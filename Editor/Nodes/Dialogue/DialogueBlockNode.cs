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
        
        public virtual bool SupportStartAction => true;
        public virtual bool SupportEndAction => true;
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            if (SupportStartAction || SupportEndAction)
                context.AddOption<bool>(IDialogueGraphNode.OP_NODE_EVENTS).WithDisplayName("Use Events").ShowInInspectorOnly().Build();
        }
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            if(!SupportStartAction && !SupportEndAction)
                return;
            
            if (GetNodeOptionByName(IDialogueGraphNode.OP_NODE_EVENTS).TryGetValue(out bool shouldUseEvents) && shouldUseEvents)
            {
                if (SupportStartAction)
                    context.AddOutputPort<ActionNode>(IDialogueGraphNode.OUT_NODE_START).WithDisplayName("Start").Build();
                
                if (SupportEndAction)
                    context.AddOutputPort<ActionNode>(IDialogueGraphNode.OUT_NODE_END).WithDisplayName("End").Build();
            }
        }
        
        public virtual void OnValidate(GraphLogger graphLogger)
        {
            IDialogueGraphNode.ValidateActionLinks(graphLogger, this, SupportStartAction, SupportEndAction);
        }
        
        public abstract T ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portMap);
        
        protected TValue GetPortValue<TValue>(IPort port) => IDialogueGraphNode.GetPortValue<TValue>(port);
        
        DialogueNodeData IDialogueGraphNode.ProcessNode(DialogueGraph graph, Dictionary<IPort, int> portMap) => ProcessNodeAsset(graph, portMap);
    }
}
