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
        public override void ProcessNode(EventNodeData dialogueNode, DialogueDirector director) => dialogueNode.voidEvent.Invoke();
    }
}
