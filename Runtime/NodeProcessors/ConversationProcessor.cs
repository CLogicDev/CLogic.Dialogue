using TMPro;
using System;
using CLogic.Utils.UI;
using UnityEngine;

namespace CLogic.Dialogue
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
        
        protected override void ProcessNode(ConversationNodeData nodeDate, DialogueDirector director)
        {
            base.ProcessNode(nodeDate, director);
            currentNode = nodeDate;
            characterNameText.text = nodeDate.characterName;
            
            float typingSpeed = nodeDate.textSpeed == 0f ? dialogueTextIterator.speedWordsPerMin : nodeDate.textSpeed;
            
            dialogueTextIterator.StartWriting(nodeDate.text, typingSpeed, null);
        }
        
        protected override bool CanProgressNode(ConversationNodeData nodeData, DialogueDirector director)
        {
            base.CanProgressNode(nodeData, director);
            
            if (!dialogueTextIterator.IsWriting)
                return true;
            
            if (!currentNode.skippable)
                return false;
            
            dialogueTextIterator.SkipAnimation();
            return false;
        }
        
        public override void HandleCancellation(DialogueNodeData nodeDate, DialogueDirector director)
        {
            dialogueTextIterator.StopWriting();
        }
    }
}
