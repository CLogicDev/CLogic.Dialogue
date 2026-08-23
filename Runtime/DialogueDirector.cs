using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CLogic.Dialogue.Provisioner;
using CLogic.Utils;
using Unity.Scripting.LifecycleManagement;

namespace CLogic.Dialogue
{
    [AutoStaticsCleanup]
    public partial class DialogueDirector : MonoBehaviour
    {
        [field: SerializeField]
        public DialogueHandle CurrentDialogue { get; private set; }
        
        public int maxDiscoveryDepth = 2;
        
        private DialogueNodeData currentNode;
        public IDialogueProcessor CurrentProcessor { get; private set; }
        
        private int currentNodeID;
        
        [NonSerialized]
        private DialogueNodeData[] nodes;
        
        private Dictionary<Type, IDialogueProcessor> nodeProcessors = new();
        private Dictionary<Type, List<IDialoguePreProcessor>> nodePreProcessors = new();
        private Dictionary<Type, List<IDialoguePostProcessor>> nodePostProcessors = new();
        
        [NoAutoStaticsCleanup]
        private static List<IDialogueProcessor> cachedSingletonProcessors;
        [NoAutoStaticsCleanup]
        private static Dictionary<Type, IDialogueProcessor> cachedSingletonMonoProcessors;
        private static GameObject monoProcessorsContainer;
        
        internal Dictionary<Hash128, ProvisionerData> provisionerLookup = new();
        
        public event Action OnDialogueStart;
        public event Action OnDialogueEnd;
        
        public bool IsPlaying => currentNode != null;
        
        private void Awake() => ResolveProcessors();
        
        #region Processor Resolution
        private void ResolveProcessors()
        {
            IEnumerable<IDialogueProcessor> childProcessors = DiscoverProcessorsInHierarchy(transform);
            IEnumerable<IDialogueProcessor> singletonProcessors = DiscoverSingletonProcessors();
            
            IEnumerable<IDialogueProcessor> processors = childProcessors.Concat(singletonProcessors);
            
            nodeProcessors.Clear();
            nodePreProcessors.Clear();
            nodePostProcessors.Clear();
            
            foreach (IDialogueProcessor processor in processors)
            {
                switch (processor)
                {
                    case IDialoguePreProcessor preProcessor:
                        if(!nodePreProcessors.TryGetValue(preProcessor.HandledType, out List<IDialoguePreProcessor> preProcessors))
                            preProcessors = nodePreProcessors[preProcessor.HandledType] = new List<IDialoguePreProcessor>();
                        
                        preProcessors.Add(preProcessor);
                        break;
                    case IDialoguePostProcessor postProcessor:
                        if(!nodePostProcessors.TryGetValue(postProcessor.HandledType, out List<IDialoguePostProcessor> postProcessors))
                            postProcessors = nodePostProcessors[postProcessor.HandledType] = new List<IDialoguePostProcessor>();
                        
                        postProcessors.Add(postProcessor);
                        break;
                    
                    default:
                        nodeProcessors.Add(processor.NodeType, processor);
                        break;
                }
            }
            
            Dictionary<Type, List<IDialoguePreProcessor>> resolvedPreProcessors = new();
            Dictionary<Type, List<IDialoguePostProcessor>> resolvedPostProcessors = new();
            
            foreach (Type nodeType in nodeProcessors.Keys)
            {
                List<IDialoguePreProcessor> preProcessors = new();
                List<IDialoguePostProcessor> postProcessors = new();
                
                foreach (var kvp in nodePreProcessors)
                {
                    if (kvp.Key.IsAssignableFrom(nodeType))
                        preProcessors.AddRange(kvp.Value);
                }
                
                foreach (var kvp in nodePostProcessors)
                {
                    if (kvp.Key.IsAssignableFrom(nodeType))
                        postProcessors.AddRange(kvp.Value);
                }
                
                preProcessors.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                postProcessors.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                
                resolvedPreProcessors[nodeType] = preProcessors;
                resolvedPostProcessors[nodeType] = postProcessors;
            }
            
            nodePreProcessors = resolvedPreProcessors;
            nodePostProcessors = resolvedPostProcessors;
        }
        
        private IEnumerable<IDialogueProcessor> DiscoverProcessorsInHierarchy(Transform parent, int currentDepth = 0)
        {
            foreach (IDialogueProcessor dialogueNodeProcessor in parent.GetComponents<IDialogueProcessor>())
                yield return dialogueNodeProcessor;
            
            if (currentDepth >= maxDiscoveryDepth)
                yield break;
            
            foreach (Transform child in parent)
            {
                foreach (Transform nestedChild in child)
                {
                    foreach (IDialogueProcessor dialogueNodeProcessor in DiscoverProcessorsInHierarchy(nestedChild, currentDepth + 1))
                    {
                        yield return dialogueNodeProcessor;
                    }
                }
            }
        }
        
        private IEnumerable<IDialogueProcessor> DiscoverSingletonProcessors()
        {
            if (monoProcessorsContainer == null)
                monoProcessorsContainer = new GameObject("Dialogue Processors");
            
            cachedSingletonMonoProcessors ??= new Dictionary<Type, IDialogueProcessor>();
            
            foreach (Type cachedMonoType in cachedSingletonMonoProcessors.Keys.ToArray())
            {
                var monoProcessor = cachedSingletonMonoProcessors[cachedMonoType] as MonoBehaviour;
                
                if(monoProcessor == null)
                    cachedSingletonMonoProcessors[cachedMonoType] = (IDialogueProcessor)monoProcessorsContainer.AddComponent(cachedMonoType);
                
                yield return cachedSingletonMonoProcessors[cachedMonoType];
            }
            
            if (cachedSingletonProcessors != null)
            {
                foreach (IDialogueProcessor cachedStaticProcessor in cachedSingletonProcessors)
                    yield return cachedStaticProcessor;
                
                yield break;
            }
            
            cachedSingletonProcessors = new List<IDialogueProcessor>();
            
            foreach (Type type in SingletonProcessorAttribute.GetSingletonProcessorTypes())
            {
                IDialogueProcessor processor;
                if (typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    processor = (IDialogueProcessor)monoProcessorsContainer.AddComponent(type);
                    cachedSingletonMonoProcessors.Add(type, processor);
                }
                else
                {
                    processor = (IDialogueProcessor)Activator.CreateInstance(type);
                    cachedSingletonProcessors.Add(processor);
                }
                
                yield return processor;
            }
        }
        #endregion
        
        public DialogueHandle PlayDialogueGraph(DialogueGraph graph) => PlayDialogueGraph(graph, null);
        public DialogueHandle PlayDialogueGraph(DialogueGraph graph, Action onFinish, bool forced = true, int? startIndex = null, bool callFinishCallback = true, bool createVisualizationContext = true)
        {
            if (!forced && IsPlaying)
                return new DialogueHandle();
            
            if (IsPlaying)
                EndDialogue(callFinishCallback);
            CurrentDialogue = new DialogueHandle(this, true, graph, onFinish);
            nodes = graph.nodes;
            
            OnDialogueStart?.Invoke();
            
            (provisionerLookup ??= new Dictionary<Hash128, ProvisionerData>()).Clear();
            foreach (ProvisionerData provisionerData in graph.provisionerData)
            {
                foreach (var kvp in provisionerData.linkedNodes)
                {
                    provisionerLookup.Add(kvp.Key, provisionerData);
                }
            }
            
            #if UNITY_EDITOR
            if(createVisualizationContext)
                SetupDebugContext(graph);
            #endif
            
            GoToNode(startIndex ?? graph.startNodeID, true);
            
            #if UNITY_EDITOR
            ShowVisualizationForNode(currentNode, CurrentProcessor);
            #endif
            return CurrentDialogue;
        }
        
        public void EndDialogue(bool callFinishCallback = true)
        {
            if (!IsPlaying)
                return;
            
            CurrentProcessor?.HandleCancellation(currentNode, this);
            currentNode = null;
            
            if (callFinishCallback)
            {
                #if UNITY_EDITOR
                CurrentContext?.Dispose();
                #endif
                
                CurrentDialogue.SetDialogueFinished();
                OnDialogueEnd?.Invoke();
            }
        }
        
        /// <summary>
        /// Tries to go to the next node
        /// </summary>
        /// <returns>Whether the director could go to the next node</returns>
        public bool GoToNextNode(bool forced = false)
        {
            if(forced)
                return GoToNode(currentNode.nextNodeID, true);
            
            return IsPlaying && GoToNode(currentNode.nextNodeID);
        }

        /// <summary>
        /// Tries to go to a specific node
        /// </summary>
        /// <returns>Whether the director could go to that node</returns>
        public bool GoToNode(int nodeID, bool forced = false)
        {
            if(!forced && !CurrentProcessor.CanProgressNode(currentNode, this))
                return false;
            
            switch (nodeID)
            {
                case -1:
                    Integrations.LogWarning("Abrupt graph ending detected. Please ensure end nodes are properly linked where the graph ends");
                    EndDialogue();
                    return false;
                case -2:
                    EndDialogue();
                    return false;
            }
            
            currentNode = GetNodeFromID(nodeID);
            currentNodeID = nodeID;
            ProcessNode(currentNode);
            
            #if UNITY_EDITOR
            ShowExecutionPath(currentNode);
            #endif
            
            return true;
        }
        
        public DialogueNodeData GetNodeFromID(int nodeID) => nodes[nodeID];
        
        public void ProcessNode(DialogueNodeData nodeData, bool fireAndForget = false)
        {
            if (nodeData is SubGraphNodeData subGraphNodeData)
            {
                CurrentProcessor = null;
                ProcessSubGraph(subGraphNodeData); // Sub graph processing has precedence over all processing logic
                return;
            }
            
            Type type = nodeData.GetType();
            
            if(!TryGetProcessorForNode(type, out IDialogueProcessor nodeProcessor))
            {
                Integrations.LogError($"No processor for type {type}");
                return;
            }
            
            if (!fireAndForget)
                CurrentProcessor = nodeProcessor;
            
            foreach (IDialoguePreProcessor processor in nodePreProcessors[type])
                processor.PreProcessInternal(nodeData, this);
            
            nodeProcessor.ProcessNode(nodeData, this);
            
            foreach (IDialoguePostProcessor processor in nodePostProcessors[type])
                processor.PostProcessInternal(nodeData, this);
        }

        public bool TryGetProcessorForNode<T>(Type type, out T processor) where T : IDialogueProcessor
        {
            if(nodeProcessors.TryGetValue(type, out IDialogueProcessor rawProcessor))
            {
                processor = (T)rawProcessor;
                return true;
            }

            Integrations.LogError($"No processor for type {type}");
            processor = default;
            return false;

        }
    }
}
