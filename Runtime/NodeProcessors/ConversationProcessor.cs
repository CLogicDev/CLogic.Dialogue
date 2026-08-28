using TMPro;
using System;
using CLogic.Dialogue.Provisioner;
using CLogic.Dialogue.Extras;
using UnityEngine;

namespace CLogic.Dialogue
{
    [Serializable]
    public class ConversationNodeData : DialogueNodeData
    {
        public const string IN_TEXT = "Text";
        
        [Provision(IN_TEXT)]
        public string text;
        public string characterName;
        public float textSpeed;
        public bool skippable;
    }
    
    public class ConversationProcessor : DialogueProcessor<ConversationNodeData>
    {
        [SerializeField]
        private TypeWriter dialogueTextIterator;
        
        [SerializeField]
        private TextMeshProUGUI characterNameText;
        
        private ConversationNodeData currentNode;
        
        protected override void ProcessNode(ConversationNodeData nodeData, DialogueDirector director)
        {
            currentNode = nodeData;
            characterNameText.text = nodeData.characterName;
            
            float typingSpeed = nodeData.textSpeed == 0f ? dialogueTextIterator.speedWordsPerMin : nodeData.textSpeed;
            
            dialogueTextIterator.StartWriting(nodeData.text, typingSpeed, null);
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
        
        protected override void HandleCancellation(ConversationNodeData nodeData, DialogueDirector director)
        {
            dialogueTextIterator.StopWriting();
        }
    }
}
