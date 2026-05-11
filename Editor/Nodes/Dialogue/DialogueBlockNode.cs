using System;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using System.Linq;
using CLogic.Dialogue;

namespace CLogic.Dialogue.Editor
{
    [Serializable]
    public abstract class DialogueBlockNode<T> : BlockNode, IDialogueGraphNode where T : DialogueNodeData
    {
        public const string OUT_NODE_END = "End";
        public const string OUT_NODE_START = "Start";
        
        public const string OP_NODE_EVENTS = "UseEvents";
        
        public virtual bool SupportStartAction => true;
        public virtual bool SupportEndAction => true;
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            if (SupportStartAction || SupportEndAction)
                context.AddOption<bool>(OP_NODE_EVENTS).WithDisplayName("Use Events").ShowInInspectorOnly().Build();
        }
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            if(!SupportStartAction && !SupportEndAction)
                return;
            
            if (GetNodeOptionByName(OP_NODE_EVENTS).TryGetValue(out bool shouldUseEvents) && shouldUseEvents)
            {
                if (SupportStartAction)
                    context.AddOutputPort<ActionNode>(OUT_NODE_START).WithDisplayName("Start").Build();
                
                if (SupportEndAction)
                    context.AddOutputPort<ActionNode>(OUT_NODE_END).WithDisplayName("End").Build();
            }
        }
        
        public virtual void OnValidate(GraphLogger graphLogger)
        {
            if(!SupportStartAction || !SupportEndAction)
                return;
            
            if (GetNodeOptionByName(OP_NODE_EVENTS).TryGetValue(out bool shouldUseEvents) && shouldUseEvents)
            {
                if (SupportStartAction)
                {
                    IPort connectedPort = GetOutputPortByName(OUT_NODE_START)?.FirstConnectedPort;
                    
                    INode connectedNode = connectedPort.GetNode();
                    if (connectedNode is not null and not ActionNode)
                        graphLogger.LogError("Start node must be connected to an action node", this);
                }
                
                if (SupportEndAction)
                {
                    IPort connectedPort = GetOutputPortByName(OUT_NODE_END)?.FirstConnectedPort;
                    
                    INode connectedNode = connectedPort.GetNode();
                    if (connectedNode is not null and not ActionNode)
                        graphLogger.LogError("End node must be connected to an action node", this);
                }
            }
        }
        
        public abstract T ProcessNodeAsset(DialogueGraph graph, Dictionary<INode, int> nodeMap);
        
        protected TValue GetPortValue<TValue>(IPort port) => IDialogueGraphNode.GetPortValue<TValue>(port);
        
        DialogueNodeData IDialogueGraphNode.ProcessNode(DialogueGraph graph, Dictionary<INode, int> nodeMap) => ProcessNodeAsset(graph, nodeMap);
        
        public void CreateActionNodeLink(T node, Dictionary<INode, int> nodeMap)
        {
            if (SupportStartAction)
            {
                IPort connectedPort = GetOutputPortByName(OUT_NODE_START)?.FirstConnectedPort;
                
                if (connectedPort != null)
                    node.startNodeActionID = nodeMap.GetValueOrDefault(connectedPort.GetNode(), -1);
            }
            
            if (SupportEndAction)
            {
                IPort connectedPort = GetOutputPortByName(OUT_NODE_END)?.FirstConnectedPort;
                
                if (connectedPort != null)
                    node.endNodeActionID = nodeMap.GetValueOrDefault(connectedPort.GetNode(), -1);
            }
        }
    }
}
