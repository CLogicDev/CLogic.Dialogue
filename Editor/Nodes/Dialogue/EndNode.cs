using System;
using System.Reflection;
using Unity.GraphToolkit.Editor;

namespace CLogic.Dialogue.Editor
{
    [Serializable, Node("Basic Nodes", "", "End Point")]
    public class EndNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            var inputPort = context.AddInputPort<IDialogueGraphNode>("End").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            
            PropertyInfo propertyInfo = inputPort.GetType().GetProperty("Capacity", BindingFlags.Instance | BindingFlags.Public);
            Type portCapacityType = propertyInfo.PropertyType;
            object multiCapacity = Enum.Parse(portCapacityType, "Multi");
            propertyInfo.SetValue(inputPort, multiCapacity);
        }
    }
}
