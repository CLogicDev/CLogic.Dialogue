using TMPro;
using System;
using CLogic.Utils.UI;
using UnityEngine;

namespace CLogic.Systems.DialogueSystem
{
    [Serializable]
    public class ConversationNodeData : DialogueNodeData
    {
        public string text;
        public string characterName;
        public float textSpeed;
        public bool skippable;
    }
    
    public class ConversationProcessor : DialogueNodeProcessor<ConversationNodeData>
    {
        [SerializeField]
        private TypeWriter dialogueTextIterator;
        
        [SerializeField]
        private TextMeshProUGUI characterNameText;
        
        private ConversationNodeData currentNode;
        
        public override void ProcessNode(ConversationNodeData dialogueNode, DialogueDirector director)
        {
            base.ProcessNode(dialogueNode, director);
            currentNode = dialogueNode;
            characterNameText.text = dialogueNode.characterName;
            
            float typingSpeed = dialogueNode.textSpeed == 0f ? dialogueTextIterator.speedWordsPerMin : dialogueNode.textSpeed;
            
            dialogueTextIterator.StartWriting(dialogueNode.text, typingSpeed, null);
        }
        
        public override bool CanProgressNode(DialogueNodeData dialogueNode, DialogueDirector director)
        {
            base.CanProgressNode(dialogueNode, director);
            
            if (!dialogueTextIterator.IsWriting)
                return true;
            
            if (!currentNode.skippable)
                return false;
            
            dialogueTextIterator.SkipAnimation();
            return false;
        }
        
        public override void HandleCancellation(DialogueNodeData dialogueNode, DialogueDirector director)
        {
            dialogueTextIterator.StopWriting();
        }
    }
}
