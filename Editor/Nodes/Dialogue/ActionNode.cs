using System;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using System.Reflection;
using CLogic.Dialogue;

namespace CLogic.Dialogue.Editor
{
    [Serializable, Node("Events", "", "Action Block")]
    public class ActionNode : DialogueContextNode<ActionNodeData>
    {
        public override bool SupportExecution => false;
        public override bool SupportStartAction => false;
        public override bool SupportEndAction => false;
        
        protected override void DefineDialoguePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<IDialogueGraphNode>(IN_EXECUTION).WithDisplayName(string.Empty).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            
            IPort port = GetInputPortByName(IN_EXECUTION);
            PropertyInfo propertyInfo = port.GetType().GetProperty("Capacity", BindingFlags.Instance | BindingFlags.Public);
            Type portCapacityType = propertyInfo.PropertyType;
            object multiCapacity = Enum.Parse(portCapacityType, "Multi");
            propertyInfo.SetValue(port, multiCapacity);
        }
        
        public override ActionNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<INode, int> nodeMap)
        {
            ActionNodeData data = new();
            
            ProcessChildBlocks(data, graph, nodeMap);
            
            return data;
        }
        
        public override void OnValidate(GraphLogger graphLogger)
        {
            if (BlockCount == 0)
                graphLogger.LogWarning("Behavior node has no blocks", this);
        }
    }
}
