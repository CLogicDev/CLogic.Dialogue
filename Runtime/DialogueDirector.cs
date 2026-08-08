using System;
using UnityEngine;
using EditorAttributes;
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
        
        [field: SerializeField]
        public List<DialogueProcessor> processorOverrides { get; private set; } = new();
        
        private DialogueNodeData currentNode;
        public IDialogueProcessor CurrentProcessor { get; private set; }
        
        private int currentNodeID;
        
        [NonSerialized]
        private DialogueNodeData[] nodes;
        
        [SerializeField, ShowField(nameof(IsUsingBakedProcessors))]
        private List<DialogueProcessor> bakedProcessors = new();
        private Dictionary<Type, IDialogueProcessor> nodeProcessors = new();
        private Dictionary<Type, IDialogueProcessor> nodeProcessorOverrides = new();
        private Dictionary<Type, List<IDialoguePreProcessor>> nodePreProcessors = new();
        private Dictionary<Type, List<IDialoguePostProcessor>> nodePostProcessors = new();
        
        [NoAutoStaticsCleanup]
        private static List<IDialogueProcessor> cachedStaticProcessors;
        [NoAutoStaticsCleanup]
        private static Dictionary<Type, IDialogueProcessor> cachedStaticMonoProcessors;
        private static GameObject monoProcessorsContainer;
        
        internal Dictionary<Hash128, ProvisionerData> provisionerLookup = new();
        
        public event Action OnDialogueStart;
        public event Action OnDialogueEnd;
        
        [ShowInInspector]
        public bool IsPlaying => currentNode != null;
        
        [ReadOnly, ShowInInspector]
        public bool IsUsingBakedProcessors => bakedProcessors.Count > 0;
        
        private void Awake()
        {
            foreach (DialogueProcessor processor in processorOverrides)
            {
                nodeProcessorOverrides.Add(processor.NodeType, processor);
            }
            
            IEnumerable<IDialogueProcessor> processorProvider = IsUsingBakedProcessors ? bakedProcessors : DiscoverProcessors(transform);
            
            nodeProcessors.Clear();
            nodePreProcessors.Clear();
            nodePostProcessors.Clear();
            foreach (IDialogueProcessor processor in processorProvider)
            {
                switch (processor)
                {
                    case IDialoguePreProcessor preProcessor:
                        if(!nodePreProcessors.TryGetValue(preProcessor.HandledType, out List<IDialoguePreProcessor> preProcessors)) 
                            preProcessors = nodePreProcessors[preProcessor.HandledType] = new List<IDialoguePreProcessor>();
                        
                        preProcessors.Add(preProcessor);
                        continue;
                    case IDialoguePostProcessor postProcessor:
                        if(!nodePostProcessors.TryGetValue(postProcessor.HandledType, out List<IDialoguePostProcessor> postProcessors))
                            postProcessors = nodePostProcessors[postProcessor.HandledType] = new List<IDialoguePostProcessor>();
                        
                        postProcessors.Add(postProcessor);
                        continue;
                }
                
                if (nodeProcessors == null || !nodeProcessorOverrides.ContainsKey(processor.NodeType))
                    nodeProcessors.Add(processor.NodeType, processor);
            }
            
            foreach (List<IDialoguePreProcessor> preProcessors in nodePreProcessors.Values)
            {
                preProcessors.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }
            
            foreach (List<IDialoguePostProcessor> postProcessors in nodePostProcessors.Values)
            {
                postProcessors.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }
        }
        
        [Button("Bake Processors")]
        public void BakeProcessors()
        {
            bakedProcessors.Clear();
            foreach (IDialogueProcessor processor in DiscoverProcessors(transform))
            {
                if (!nodeProcessorOverrides.ContainsKey(processor.NodeType))
                    bakedProcessors.Add(processor as DialogueProcessor); // If processors are discovered they must implement DialogueNodeProcessor
            }
        }
        
        public IEnumerable<IDialogueProcessor> DiscoverProcessors(Transform parent, int currentDepth = 0)
        {
            //Check parent components
            foreach (IDialogueProcessor dialogueNodeProcessor in parent.GetComponents<IDialogueProcessor>())
            {
                yield return dialogueNodeProcessor;
            }
            
            //Don't check children is depth is exceeded
            if (currentDepth >= maxDiscoveryDepth)
                yield break;
            
            foreach (Transform child in parent)
            {
                //Check child components
                foreach (IDialogueProcessor dialogueNodeProcessor in child.GetComponents<IDialogueProcessor>())
                {
                    yield return dialogueNodeProcessor;
                }
                
                //Recursively check nested children
                foreach (Transform nestedChild in child)
                {
                    foreach (IDialogueProcessor dialogueNodeProcessor in DiscoverProcessors(nestedChild, currentDepth + 2)) // Depth is +2 since child is checked then the nested child
                    {
                        yield return dialogueNodeProcessor;
                    }
                }
            }
            
            if (currentDepth > 0)
                yield break;
            
            if (monoProcessorsContainer == null)
                monoProcessorsContainer = new GameObject("Dialogue Processors");
            
            cachedStaticMonoProcessors ??= new Dictionary<Type, IDialogueProcessor>();
            
            foreach (Type cachedMonoType in cachedStaticMonoProcessors.Keys.ToArray())
            {
                var monoProcessor = cachedStaticMonoProcessors[cachedMonoType] as MonoBehaviour;
                
                if(monoProcessor == null)
                    cachedStaticMonoProcessors[cachedMonoType] = (IDialogueProcessor)monoProcessorsContainer.AddComponent(cachedMonoType);
                
                yield return cachedStaticMonoProcessors[cachedMonoType];
            }
            
            if (cachedStaticProcessors != null)
            {
                foreach (IDialogueProcessor cachedStaticProcessor in cachedStaticProcessors)
                    yield return cachedStaticProcessor;
                
                yield break;
            }
            
            cachedStaticProcessors = new List<IDialogueProcessor>();
            
            foreach (Type type in StaticProcessorAttribute.GetStaticProcessorTypes())
            {
                IDialogueProcessor processor;
                if (typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    processor = (IDialogueProcessor)monoProcessorsContainer.AddComponent(type);
                    cachedStaticMonoProcessors.Add(type, processor);
                }
                else
                {
                    processor = (IDialogueProcessor)Activator.CreateInstance(type);
                    cachedStaticProcessors.Add(processor);
                }
                
                yield return processor;
            }
        }
        
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
        
        // Not local function to allow access from sub graph handler
        [Button(serializeParameters: false)]
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
            
            if (nodePreProcessors.TryGetValue(typeof(DialogueNodeData), out var globalPreProcessors))
                foreach (var processor in globalPreProcessors)
                    processor.PreProcessInternal(nodeData, this);
            
            if (nodePreProcessors.TryGetValue(nodeData.GetType(), out var preProcessors))
                foreach (var processor in preProcessors)
                    processor.PreProcessInternal(nodeData, this);
            
            nodeProcessor.ProcessNode(nodeData, this);
            
            if (nodePostProcessors.TryGetValue(typeof(DialogueNodeData), out var globalPostProcessors))
                foreach (var processor in globalPostProcessors)
                    processor.PostProcessInternal(nodeData, this);
            
            if (nodePostProcessors.TryGetValue(nodeData.GetType(), out var postProcessors))
                foreach (var processor in postProcessors)
                    processor.PostProcessInternal(nodeData, this);
        }

        public bool TryGetProcessorForNode<T>(Type type, out T processor) where T : IDialogueProcessor
        {
            if(nodeProcessorOverrides.TryGetValue(type, out IDialogueProcessor rawProcessor) || nodeProcessors.TryGetValue(type, out rawProcessor))
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
