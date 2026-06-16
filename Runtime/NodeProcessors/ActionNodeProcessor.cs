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
        public bool CanProgressNode(DialogueNodeData nodeData, DialogueDirector director) => true;
        
        public void ProcessNode(DialogueNodeData nodeData, DialogueDirector director)
        {
            var actionNodeData = (ActionNodeData)nodeData;
            
            foreach (DialogueNodeData childBlock in actionNodeData.childBlocks)
            {
                director.ProcessNode(childBlock, true);
            }
        }
        public void HandleCancellation(DialogueNodeData nodeData, DialogueDirector director) {}
    }
}
