using System;
using Unity.GraphToolkit.Editor;

namespace CLogic.Dialogue.Editor
{
    [Serializable, Node("Basic Nodes", "Packages/dev.clogic.dialogue/Icons/End Node.svg", "End Point")]
    public class EndNode : Node
    {
        public override void OnEnable() => Tooltip = "The exit point of the graph";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<IDialogueGraphNode>("Exit").WithCapacity(PortCapacity.Multi).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }
    }
}
