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
        
        [NonSerialized] public IReadOnlyDictionary<Type, IDialogueProcessor> nodeProcessors;
        [NonSerialized] public IReadOnlyDictionary<Type, List<IDialoguePreProcessor>> nodePreProcessors;
        [NonSerialized] public IReadOnlyDictionary<Type, List<IDialoguePostProcessor>> nodePostProcessors;
        
        [NoAutoStaticsCleanup]
        private static List<IDialogueProcessorBase> cachedSingletonProcessors;
        [NoAutoStaticsCleanup]
        private static Dictionary<Type, IDialogueProcessorBase> cachedSingletonMonoProcessors;
        private static GameObject monoProcessorsContainer;
        
        internal Dictionary<Hash128, ProvisionerData> provisionerLookup = new();
        
        public event Action OnDialogueStart;
        public event Action OnDialogueEnd;
        
        public bool IsPlaying => CurrentNode != null;
        
        private void Awake() => ResolveProcessors();
        
        #region Processor Resolution
        private void ResolveProcessors()
        {
            IEnumerable<IDialogueProcessorBase> childProcessors = DiscoverProcessorsInHierarchy(transform);
            IEnumerable<IDialogueProcessorBase> singletonProcessors = DiscoverSingletonProcessors();
            
            IEnumerable<IDialogueProcessorBase> resolvedProcessors = childProcessors.Concat(singletonProcessors);
            
            Dictionary<Type, IDialogueProcessor> processors = new();
            Dictionary<Type, List<IDialoguePreProcessor>> preProcessors = new();
            Dictionary<Type, List<IDialoguePostProcessor>> postProcessors = new();
            
            foreach (IDialogueProcessorBase processor in resolvedProcessors)
            {
                switch (processor)
                {
                    case IDialoguePreProcessor preProcessor:
                        if(!preProcessors.TryGetValue(preProcessor.NodeType, out List<IDialoguePreProcessor> preProcessorList))
                            preProcessorList = preProcessors[preProcessor.NodeType] = new List<IDialoguePreProcessor>();
                        
                        preProcessorList.Add(preProcessor);
                        break;
                    case IDialoguePostProcessor postProcessor:
                        if(!postProcessors.TryGetValue(postProcessor.NodeType, out List<IDialoguePostProcessor> postProcessorList))
                            postProcessorList = postProcessors[postProcessor.NodeType] = new List<IDialoguePostProcessor>();
                        
                        postProcessorList.Add(postProcessor);
                        break;
                    
                    default:
                        processors.Add(processor.NodeType, (IDialogueProcessor)processor);
                        break;
                }
            }
            
            Dictionary<Type, List<IDialoguePreProcessor>> resolvedPreProcessors = new();
            Dictionary<Type, List<IDialoguePostProcessor>> resolvedPostProcessors = new();
            
            //
            foreach (Type nodeType in processors.Keys.Append(typeof(SubGraphNodeData)))
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
            foreach (IDialogueProcessor processor in parent.GetComponents<IDialogueProcessor>())
                yield return processor;
            
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
        
        private IEnumerable<IDialogueProcessorBase> DiscoverSingletonProcessors()
        {
            if (monoProcessorsContainer == null)
                monoProcessorsContainer = new GameObject("Dialogue Processors");
            
            cachedSingletonMonoProcessors ??= new Dictionary<Type, IDialogueProcessorBase>();
            
            foreach (Type cachedMonoType in cachedSingletonMonoProcessors.Keys.ToArray())
            {
                var monoProcessor = cachedSingletonMonoProcessors[cachedMonoType] as MonoBehaviour;
                
                if(monoProcessor == null)
                    cachedSingletonMonoProcessors[cachedMonoType] = (IDialogueProcessorBase)monoProcessorsContainer.AddComponent(cachedMonoType);
                
                yield return cachedSingletonMonoProcessors[cachedMonoType];
            }
            
            if (cachedSingletonProcessors != null)
            {
                foreach (IDialogueProcessorBase cachedStaticProcessor in cachedSingletonProcessors)
                    yield return cachedStaticProcessor;
                
                yield break;
            }
            
            cachedSingletonProcessors = new List<IDialogueProcessorBase>();
            
            foreach (Type type in SingletonProcessorAttribute.GetSingletonProcessorTypes())
            {
                IDialogueProcessorBase processor;
                if (typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    processor = (IDialogueProcessorBase)monoProcessorsContainer.AddComponent(type);
                    cachedSingletonMonoProcessors.Add(type, processor);
                }
                else
                {
                    processor = (IDialogueProcessorBase)Activator.CreateInstance(type);
                    cachedSingletonProcessors.Add(processor);
                }
                
                yield return processor;
            }
        }
        #endregion
        
        /// <summary>
        /// Plays a given graph. Intended for use using Unity Events
        /// </summary>
        public void UPlayDialogueGraph(DialogueGraph graph) => PlayDialogueGraph(graph, null);
        
        /// <inheritdoc cref="PlayDialogueGraph(CLogic.Dialogue.DialogueGraph, Action, bool, int?, bool, bool)"/>
        public DialogueHandle PlayDialogueGraph(DialogueGraph graph) => PlayDialogueGraph(graph, null);
        
        /// <summary>
        /// Plays a given graph
        /// </summary>
        /// <param name="graph">The graph to play</param>
        /// <param name="onFinish">Callback when the end node is reached</param>
        /// <param name="forced">Whether to play even is another graph is already playing. This will override the previous graph</param>
        /// <returns></returns>
        public DialogueHandle PlayDialogueGraph(DialogueGraph graph, Action onFinish, bool forced = true, int? startIndex = null)
        {
            if (!forced && IsPlaying)
                return new DialogueHandle();
            
            if (IsPlaying)
                EndDialogue();
            CurrentDialogue = new DialogueHandle(this, true, graph, onFinish);
            
            LoadGraph(graph, true);
            
            OnDialogueStart?.Invoke();
            
            GoToNode(startIndex ?? graph.startNodeID, true);
            
            #if UNITY_EDITOR
            ShowVisualizationForNode(CurrentNode, CurrentProcessor);
            #endif
            return CurrentDialogue;
        }
        
        internal void LoadGraph(DialogueGraph graph, bool createVisualizationContext = true)
        {
            nodes = graph.nodes;
            
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
        }
        
        public void EndDialogue(bool endGracefully = true)
        {
            if (!IsPlaying)
                return;
            
            if(!endGracefully)
                CurrentProcessor?.HandleCancellation(CurrentNode, this);
            CurrentNode = null;
            
            #if UNITY_EDITOR
            CurrentContext?.Dispose();
            
            if (CurrentDialogue.executionFrames != null)
            {
                while (CurrentDialogue.executionFrames.TryPop(out SubGraph subGraph))
                {
                    subGraph.visualizationContext?.Dispose();
                }
            }
            #endif
                
            CurrentDialogue.SetDialogueFinished();
            OnDialogueEnd?.Invoke();
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
                    if(IsInSubGraph)
                        HandleSubGraphFinished();
                    else
                        EndDialogue(false);
                    return false;
                case -2:
                    if(IsInSubGraph)
                        HandleSubGraphFinished();
                    else
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
        
        /// <summary>
        /// Calls the appropriate processor for the given node. 
        /// </summary>
        /// <param name="nodeData">The node to process</param>
        /// <param name="fireAndForget">Does not make the processor the active one if set</param>
        /// <exception cref="ArgumentException">The given node had no processor</exception>
        public void ProcessNode(DialogueNodeData nodeData, bool fireAndForget = false)
        {
            if (nodeData is SubGraphNodeData subGraphNodeData)
            {
                // Subgraph change execution frame, so post processing needs to happen before
                foreach (IDialoguePostProcessor processor in nodePostProcessors[CurrentNode.GetType()])
                    processor.PostProcessInternal(CurrentNode, this);
                
                ProcessSubGraph(subGraphNodeData); // Sub graph processing has precedence over all processing logic
                return;
            }
            Type type = nodeData.GetType();
            
            if(!TryGetProcessorForNode(type, out IDialogueProcessor nodeProcessor))
                throw new ArgumentException($"No processor for type {type}", nameof(nodeData));
            
            if (!fireAndForget)
                CurrentProcessor = nodeProcessor;
            
            foreach (IDialoguePreProcessor processor in nodePreProcessors[type])
                processor.PreProcessInternal(nodeData, this);
            
            nodeProcessor.ProcessNode(nodeData, this);
        }
        
        /// <summary>
        /// Attempts to retrieve the processor for a given node
        /// </summary>
        /// <param name="type">The type of the node to retrieve the processor for</param>
        /// <param name="processor">Instance of the processor if found</param>
        /// <typeparam name="T">The type of the processor handling the given node</typeparam>
        /// <returns>Whether the processor could be found</returns>
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
        
        /// <summary>
        /// Retrieves the processor for a given node
        /// </summary>
        /// <typeparam name="T">The type of the processor handling the given node</typeparam>
        /// <returns>Instance of the processor found</returns>
        public T GetProcessorForNode<T>(Type type) => (T)nodeProcessors[type];
    }
}
