using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLogic.Systems.DialogueSystem
{    
    [Serializable]
    public class ActionNodeData : ContextNodeData { }

    public class ActionNodeProcessor : IDialogueNodeProcessor
    {
        public Type NodeType => typeof(ActionNodeData);
        public bool CanProgressNode(DialogueNodeData dialogueNode, DialogueDirector director) => true;
        
        public void ProcessNode(DialogueNodeData dialogueNode, DialogueDirector director)
        {
            ActionNodeData actionNodeData = (ActionNodeData)dialogueNode;
            
            foreach (var childBlock in actionNodeData.childBlocks)
                director.ProcessNode(childBlock, true);
        }
        public void HandleCancellation(DialogueNodeData dialogueNode, DialogueDirector director) {}
    }
}