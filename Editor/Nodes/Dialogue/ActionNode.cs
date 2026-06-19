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
            var portBuilder = context.AddInputPort<IDialogueGraphNode>(IN_EXECUTION).WithDisplayName(string.Empty).WithConnectorUI(PortConnectorUI.Arrowhead);
            
            #if UNITY_6000_6_OR_NEWER
            portBuilder.WithCapacity(PortCapacity.Multi).Build();
            #else
            IPort port = portBuilder.Build();
            PropertyInfo propertyInfo = port.GetType().GetProperty("Capacity", BindingFlags.Instance | BindingFlags.Public);
            Type portCapacityType = propertyInfo.PropertyType;
            object multiCapacity = Enum.Parse(portCapacityType, "Multi");
            propertyInfo.SetValue(port, multiCapacity);
            #endif
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
    }
}
