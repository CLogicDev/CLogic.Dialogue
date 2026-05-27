using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLogic.Dialogue
{
    [Serializable]
    public class ActionNodeData : ContextNodeData
    {}
    
    public class ActionNodeProcessor : IDialogueNodeProcessor
    {
        public Type NodeType => typeof(ActionNodeData);
        public bool CanProgressNode(DialogueNodeData dialogueNode, DialogueDirector director) => true;
        
        public void ProcessNode(DialogueNodeData nodeDate, DialogueDirector director)
        {
            var actionNodeData = (ActionNodeData)nodeDate;
            
            foreach (BlockNodeData childBlock in actionNodeData.childBlocks)
            {
                director.ProcessNode(childBlock, true);
            }
        }
        public void HandleCancellation(DialogueNodeData nodeDate, DialogueDirector director) {}
    }
}
