using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

namespace CLogic.Dialogue.Editor
{
    [Serializable, UseWithContext(typeof(ActionNode)), Node("Events", "", "Log")]
    public class LogNode : DialogueBlockNode<LogNodeData>
    {
        public const string IN_MESSAGE = "Message";

        public override void OnEnable() => Tooltip = "Logs a message to the console";

        public override LogNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portmap) => new()
        {
            message = GetPortValue<string>(GetInputPortByName(IN_MESSAGE))
        };

        protected override void OnDefinePorts(IPortDefinitionContext context) => context.AddInputPort<string>(IN_MESSAGE).Build();
    }
}
