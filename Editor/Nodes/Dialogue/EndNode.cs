using System;
using Unity.GraphToolkit.Editor;

namespace CLogic.Dialogue.Editor
{
    [Serializable, Node("Basic Nodes")]
    public class EndNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context) => context.AddInputPort<IDialogueGraphNode>("End").WithConnectorUI(PortConnectorUI.Arrowhead).Build();
    }
}
