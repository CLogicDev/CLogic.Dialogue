using System;
using CLogic.Systems.ScriptableEventSystem;

namespace CLogic.Dialogue
{
    [Serializable]
    public class EventNodeData : BlockNodeData
    {
        public VoidEvent voidEvent;
    }
    
    public class EventProcessor : DialogueProcessor<EventNodeData>
    {
        protected override void ProcessNode(EventNodeData nodeData, DialogueDirector director) => nodeData.voidEvent.Invoke();
    }
}
