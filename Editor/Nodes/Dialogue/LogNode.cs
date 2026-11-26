using System;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;

namespace CLogic.Dialogue.Editor
{
    [Serializable, UseWithContext(typeof(ActionNode)), Node("Events", "", "Log")]
    public class LogNode : DialogueBlockNode<LogNodeData>
    {
        public const string IN_MESSAGE = "Message";

        public override void OnEnable() => Tooltip = "Logs a message to the console";

        protected override void OnDefinePorts(IPortDefinitionContext context) => context.AddInputPort<string>(IN_MESSAGE).Build();

        public override LogNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portmap) => new()
        {
            message = GetPortValue<string>(GetInputPortByName(IN_MESSAGE))
        };
    }
}
