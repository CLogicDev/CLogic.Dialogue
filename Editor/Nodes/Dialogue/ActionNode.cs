using System;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using System.Reflection;
using CLogic.Dialogue;

namespace CLogic.Dialogue.Editor
{
    [Serializable, Node("Events", "", "Action Block")]
    public class ActionNode : DialogueContextNode<ActionNodeData>, IConnectionValidator
    {
        public override bool SupportExecution => false;
        public override bool SupportStartAction => false;
        public override bool SupportEndAction => false;
        
        protected override void DefineDialoguePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<IDialogueGraphNode>(IN_EXECUTION).WithDisplayName(string.Empty).WithConnectorUI(PortConnectorUI.Arrowhead).WithCapacity(PortCapacity.Multi).Build();
        }
        
        public override ActionNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portMap)
        {
            ActionNodeData data = new();
            
            ProcessChildBlocks(data, graph, portMap);
            
            return data;
        }
        
        public override void OnValidate(GraphLogger graphLogger)
        {
            base.OnValidate(graphLogger);
            
            if (BlockCount == 0)
                graphLogger.LogWarning("Behavior node has no blocks", this);
        }
        public bool? CanConnect(IPort output, IPort input) => output.Name is DialogueNode<DialogueNodeData>.OUT_NODE_START or DialogueNode<DialogueNodeData>.OUT_NODE_END;
    }
}
