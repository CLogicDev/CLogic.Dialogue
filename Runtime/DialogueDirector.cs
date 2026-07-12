using System;
using UnityEngine;
using EditorAttributes;
using System.Collections.Generic;
using System.Linq;
using CLogic.Utils;

namespace CLogic.Dialogue
{
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
            foreach (IDialogueProcessor processor in processorProvider)
            {
                if (nodeProcessors == null || !nodeProcessorOverrides.ContainsKey(processor.NodeType))
                    nodeProcessors.Add(processor.NodeType, processor);
            }
            
            nodeProcessors.Add(typeof(ActionNodeData), new ActionProcessor()); // Action node processor is a special processor type
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
        }
        
        public DialogueHandle PlayDialogueGraph(DialogueGraph graph) => PlayDialogueGraph(graph, null);
        public DialogueHandle PlayDialogueGraph(DialogueGraph graph, Action onFinish, bool forced = true, int? startIndex = null)
        {
            if (!forced && IsPlaying)
                return new DialogueHandle();
            
            if (IsPlaying)
                EndDialogue();
            
            CurrentDialogue = new DialogueHandle(this, true, graph, onFinish);
            nodes = graph.nodes;
            
            OnDialogueStart?.Invoke();
            
            #if UNITY_EDITOR
            SetupDebugContext(graph);
            #endif
            
            GoToNode(startIndex ?? graph.startNodeID, true);
            
            #if UNITY_EDITOR
            ShowVisualizationForNode(currentNode, CurrentProcessor);
            #endif
            return CurrentDialogue;
        }
        
        // Not local function to allow access from sub graph handler
        [Button]
        public void EndDialogue()
        {
            if (!IsPlaying)
                return;
            
            CurrentProcessor?.HandleCancellation(currentNode, this);
            currentNode = null;
            
            CurrentDialogue.SetDialogueFinished();
            OnDialogueEnd?.Invoke();
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
        
        public void ProcessNode(DialogueNodeData node, bool fireAndForget = false)
        {
            if (node is SubGraphNodeData subGraphNodeData)
            {
                ProcessSubGraph(subGraphNodeData); // Sub graph processing has precedence over all processing logic
                return;
            }
            
            Type type = node.GetType();
            
            if(!TryGetProcessorForNode(type, out IDialogueProcessor processor))
            {
                Integrations.LogError($"No processor for type {type}");
                return;
            }
            
            if (!fireAndForget)
                CurrentProcessor = processor;
            
            processor.ProcessNode(node, this);
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
