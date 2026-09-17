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
        public int maxDiscoveryDepth = 2;
        
        public DialogueHandle CurrentDialogue { get; private set; }
        
        public IDialogueProcessor CurrentProcessor { get; private set; }
        
        public DialogueNodeData CurrentNode { get; private set; }
        private int currentNodeID;
        
        [NonSerialized] private DialogueNodeData[] nodes;
        
        private IReadOnlyDictionary<Type, IDialogueProcessor> nodeProcessors;
        
        [NonSerialized] public IReadOnlyDictionary<Type, List<IDialoguePreProcessor>> nodePreProcessors;
        [NonSerialized] public IReadOnlyDictionary<Type, List<IDialoguePostProcessor>> nodePostProcessors;
        
        [NoAutoStaticsCleanup]
        private static List<IDialogueProcessor> cachedSingletonProcessors;
        [NoAutoStaticsCleanup]
        private static Dictionary<Type, IDialogueProcessor> cachedSingletonMonoProcessors;
        private static GameObject monoProcessorsContainer;
        
        internal Dictionary<Hash128, ProvisionerData> provisionerLookup = new();
        
        public event Action OnDialogueStart;
        public event Action OnDialogueEnd;
        
        public bool IsPlaying => CurrentNode != null;
        
        private void Awake() => ResolveProcessors();
        
        #region Processor Resolution
        private void ResolveProcessors()
        {
            IEnumerable<IDialogueProcessor> childProcessors = DiscoverProcessorsInHierarchy(transform);
            IEnumerable<IDialogueProcessor> singletonProcessors = DiscoverSingletonProcessors();
            
            IEnumerable<IDialogueProcessor> resolvedProcessors = childProcessors.Concat(singletonProcessors);
            
            Dictionary<Type, IDialogueProcessor> processors = new();
            Dictionary<Type, List<IDialoguePreProcessor>> preProcessors = new();
            Dictionary<Type, List<IDialoguePostProcessor>> postProcessors = new();
            
            foreach (IDialogueProcessor processor in resolvedProcessors)
            {
                switch (processor)
                {
                    case IDialoguePreProcessor preProcessor:
                        if(!preProcessors.TryGetValue(preProcessor.HandledType, out List<IDialoguePreProcessor> preProcessorList))
                            preProcessorList = preProcessors[preProcessor.HandledType] = new List<IDialoguePreProcessor>();
                        
                        preProcessorList.Add(preProcessor);
                        break;
                    case IDialoguePostProcessor postProcessor:
                        if(!postProcessors.TryGetValue(postProcessor.HandledType, out List<IDialoguePostProcessor> postProcessorList))
                            postProcessorList = postProcessors[postProcessor.HandledType] = new List<IDialoguePostProcessor>();
                        
                        postProcessorList.Add(postProcessor);
                        break;
                    
                    default:
                        processors.Add(processor.NodeType, processor);
                        break;
                }
            }
            
            Dictionary<Type, List<IDialoguePreProcessor>> resolvedPreProcessors = new();
            Dictionary<Type, List<IDialoguePostProcessor>> resolvedPostProcessors = new();
            
            foreach (Type nodeType in processors.Keys)
            {
                List<IDialoguePreProcessor> preProcessorSort = new();
                List<IDialoguePostProcessor> postProcessorSort = new();
                
                foreach (var kvp in preProcessors)
                {
                    if (kvp.Key.IsAssignableFrom(nodeType))
                        preProcessorSort.AddRange(kvp.Value);
                }
                
                foreach (var kvp in postProcessors)
                {
                    if (kvp.Key.IsAssignableFrom(nodeType))
                        postProcessorSort.AddRange(kvp.Value);
                }
                
                preProcessorSort.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                postProcessorSort.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                
                resolvedPreProcessors[nodeType] = preProcessorSort;
                resolvedPostProcessors[nodeType] = postProcessorSort;
            }
            
            nodeProcessors = processors;
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
                foreach (IDialogueProcessor dialogueNodeProcessor in DiscoverProcessorsInHierarchy(child, currentDepth + 1))
                {
                    yield return dialogueNodeProcessor;
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
            ShowVisualizationForNode(CurrentNode, CurrentProcessor);
            #endif
            return CurrentDialogue;
        }
        
        public void EndDialogue(bool callFinishCallback = true)
        {
            if (!IsPlaying)
                return;
            
            CurrentProcessor?.HandleCancellation(CurrentNode, this);
            CurrentNode = null;
            
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
        public bool GoToNextNode(bool forced = false) => IsPlaying && GoToNode(CurrentNode.nextNodeID, forced);
        
        /// <summary>
        /// Tries to go to a specific node
        /// </summary>
        /// <returns>Whether the director could go to that node</returns>
        public bool GoToNode(int nodeID, bool forced = false)
        {
            if(!forced && !CurrentProcessor.CanProgressNode(CurrentNode, this))
                return false;

            if(CurrentNode != null)
            {
                foreach (IDialoguePostProcessor processor in nodePostProcessors[CurrentNode.GetType()])
                    processor.PostProcessInternal(CurrentNode, this);
            }

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
            
            CurrentNode = GetNodeFromID(nodeID);
            currentNodeID = nodeID;
            ProcessNode(CurrentNode);
            
            #if UNITY_EDITOR
            ShowExecutionPath(CurrentNode);
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
        }

        public bool TryGetProcessorForNode<T>(Type type, out T processor) where T : IDialogueProcessor
        {
            if(nodeProcessors.TryGetValue(type, out IDialogueProcessor rawProcessor))
            {
                processor = (T)rawProcessor;
                return true;
            }

            processor = default;
            return false;
        }
        
        public T GetProcessorForNode<T>(Type type) => (T)nodeProcessors[type];
    }
}
