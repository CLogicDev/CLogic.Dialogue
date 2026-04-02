using System;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using CLogic.Systems.ScriptableEventSystem;

namespace CLogic.Systems.DialogueSystem.Editor
{
    [Serializable, UseWithContext(typeof(ActionNode))]
    [Node("Events")]
    public class EventNode : DialogueBlockNode<EventNodeData>
    {
        public const string IN_EVENT = "Event";

        public override EventNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<INode, int> nodeMap) => new()
        {
            voidEvent = GetPortValue<VoidEvent>(GetInputPortByName(IN_EVENT))
        };

        protected override void OnDefinePorts(IPortDefinitionContext context) => context.AddInputPort<VoidEvent>(IN_EVENT).Build();
    }
}