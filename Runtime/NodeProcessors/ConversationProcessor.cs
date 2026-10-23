using TMPro;
using System;
using UnityEngine;
using CLogic.Dialogue.Provisioner;
using CLogic.Dialogue.Extras;

namespace CLogic.Dialogue
{
    [Serializable]
    public class ConversationNodeData : DialogueNodeData
    {
        public const string IN_TEXT = "Text";

        [Provision(IN_TEXT)] public string text;
        public string characterName;
        public float textSpeed;
        public bool skippable;
    }

    [AddComponentMenu("CLogic/Dialogue/Processors/Conversation Processor")]
    public class ConversationProcessor : DialogueProcessor<ConversationNodeData>
    {
        [SerializeField] private TypeWriter dialogueTextWriter;
        [SerializeField] private TextMeshProUGUI characterNameText;

        private ConversationNodeData currentNode;

        protected override void ProcessNode(ConversationNodeData nodeData, DialogueDirector director)
        {
            currentNode = nodeData;
            characterNameText.text = nodeData.characterName;

            float typingSpeed = nodeData.textSpeed == 0f ? dialogueTextWriter.speedWordsPerMin : nodeData.textSpeed;

            dialogueTextWriter.StartWriting(nodeData.text, typingSpeed, null);
        }

        protected override bool CanProgressNode(ConversationNodeData nodeData, DialogueDirector director)
        {
            base.CanProgressNode(nodeData, director);

            if (!dialogueTextWriter.IsWriting)
                return true;

            if (!currentNode.skippable)
                return false;

            dialogueTextWriter.SkipAnimation();
            return false;
        }

        protected override void HandleCancellation(ConversationNodeData nodeData, DialogueDirector director) => dialogueTextWriter.StopWriting();
    }
}
