using System;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using CLogic.Dialogue;
using CLogic.Systems.ScriptableEventSystem;

namespace CLogic.Dialogue.Editor
{
    [Serializable, UseWithContext(typeof(ActionNode)), Node("Events", "", "Event Raiser")]
    public class EventNode : DialogueBlockNode<EventNodeData>
    {
        public override bool SupportStartAction => false;
        public override bool SupportEndAction => false;
        
        public const string IN_EVENT = "Event";
        
        public override EventNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portMap) => new()
        {
            voidEvent = GetPortValue<VoidEvent>(GetInputPortByName(IN_EVENT))
        };
        
        protected override void OnDefinePorts(IPortDefinitionContext context) => context.AddInputPort<VoidEvent>(IN_EVENT).Build();
    }
}
