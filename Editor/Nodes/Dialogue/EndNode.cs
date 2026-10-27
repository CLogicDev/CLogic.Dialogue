using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;

namespace CLogic.Dialogue.Editor
{
    [Serializable, Node("Basic", "Packages/dev.clogic.dialogue/EditorResources/Textures/Icons/EndNode.svg", "End Point")]
    public class EndNode : Node
    {
        public override void OnEnable()
        {
            DefaultColor = Color.softRed;
            Tooltip = "The exit point of the graph";
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<IDialogueGraphNode>("Exit").WithCapacity(PortCapacity.Multi).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }
    }
}
