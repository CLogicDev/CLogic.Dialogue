using System;
using Unity.GraphToolkit.Editor;

namespace CLogic.Dialogue.Editor
{
    [Serializable, Node("Basic Nodes", "Packages/dev.clogic.dialogue/Icons/End Node.svg", "End Point")]
    public class EndNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<IDialogueGraphNode>("Exit").WithCapacity(PortCapacity.Multi).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }
    }
}
