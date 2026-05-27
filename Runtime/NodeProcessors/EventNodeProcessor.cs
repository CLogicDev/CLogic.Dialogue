using System;
using CLogic.Systems.ScriptableEventSystem;

namespace CLogic.Dialogue
{
    [Serializable]
    public class EventNodeData : BlockNodeData
    {
        public VoidEvent voidEvent;
    }
    
    public class EventNodeProcessor : DialogueNodeProcessor<EventNodeData>
    {
        protected override void ProcessNode(EventNodeData nodeDate, DialogueDirector director) => nodeDate.voidEvent.Invoke();
    }
}
