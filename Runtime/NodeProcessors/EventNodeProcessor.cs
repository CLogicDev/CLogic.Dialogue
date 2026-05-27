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
        protected override void ProcessNode(EventNodeData nodeData, DialogueDirector director) => nodeData.voidEvent.Invoke();
    }
}
