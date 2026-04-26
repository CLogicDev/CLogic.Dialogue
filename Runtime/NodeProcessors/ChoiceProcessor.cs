using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CLogic.Dialogue
{
    [Serializable]
    public class BranchNodeData : ContextNodeData
    {}
    
    [Serializable]
    public class ChoiceNodeData : BlockNodeData
    {
        public string choiceText;
        
        #if CLOGIC_CONDITIONALS
        public Conditionals.ConditionalEvaluator conditional;
        #endif
        
    }
    
    public class ChoiceProcessor : DialogueNodeProcessor<BranchNodeData>
    {
        [SerializeField]
        private GameObject choiceButtonPrefab;
        [SerializeField]
        private RectTransform choiceContainer;
        
        public override void ProcessNode(BranchNodeData dialogueNode, DialogueDirector director)
        {
            for (int i = 0; i < dialogueNode.childBlocks.Count; i++)
            {
                var choiceNode = dialogueNode.childBlocks[i] as ChoiceNodeData;
                
                GameObject buttonObject = Instantiate(choiceButtonPrefab, choiceContainer);
                buttonObject.SetActive(false); // Fixes delay with interactable state
                
                var button = buttonObject.GetComponent<Button>();
                var buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
                
                buttonText.text = choiceNode.choiceText;
                
                int index = i;
                button.onClick.AddListener(() => SelectChoice(index, dialogueNode, director));
                
                #if CLOGIC_CONDITIONALS
                Debug.Log(choiceNode.conditional == null || choiceNode.conditional.Evaluate());
                Debug.Log(choiceNode.choiceText);
                button.interactable = choiceNode.conditional == null || choiceNode.conditional.Evaluate();
                #endif
                
                buttonObject.SetActive(true);
            }
        }
        
        public override bool CanProgressNode(DialogueNodeData dialogueNode, DialogueDirector director) => false;
        
        private void SelectChoice(int choiceIndex, BranchNodeData dialogueNode, DialogueDirector director)
        {
            director.GoToNode(dialogueNode.childBlocks[choiceIndex].nextNodeID, true);
            DestroyChoiceButtons();
        }
        
        private void DestroyChoiceButtons()
        {
            foreach (Transform choiceButton in choiceContainer)
            {
                Destroy(choiceButton.gameObject);
            }
        }
        
        public override void HandleCancellation(DialogueNodeData dialogueNode, DialogueDirector director) => DestroyChoiceButtons();
    }
}
