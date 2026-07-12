using TMPro;
using System;
using CLogic.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace CLogic.Dialogue
{
    [Serializable]
    public class BranchNodeData : ContextNodeData
    {
        #if CLOGIC_CONDITIONALS
        public bool isAutoChoice;
        #endif
    }
    
    [Serializable]
    public class ChoiceNodeData : BlockNodeData
    {
        public string choiceText;
        
        #if CLOGIC_CONDITIONALS
        public Conditionals.ConditionalEvaluator conditional;
        #endif
        
    }
    
    public class ChoiceProcessor : DialogueProcessor<BranchNodeData>
    {
        [SerializeField]
        private GameObject choiceButtonPrefab;
        [SerializeField]
        private RectTransform choiceContainer;
        
        protected override void ProcessNode(BranchNodeData nodeData, DialogueDirector director)
        {
            #if CLOGIC_CONDITIONALS
            if(nodeData.isAutoChoice)
            {
                HandleAutoChoice(nodeData, director);
                return;
            }
            #endif
            
            HandleChoice(nodeData, director);
        }

        private void HandleChoice(BranchNodeData dialogueNode, DialogueDirector director)
        {
            foreach (DialogueNodeData dialogueNodeData in dialogueNode.childBlocks)
            {
                var choiceNode = dialogueNodeData as ChoiceNodeData;
                
                GameObject buttonObject = Instantiate(choiceButtonPrefab, choiceContainer);
                buttonObject.SetActive(false); // Fixes delay with interactable state
                
                var button = buttonObject.GetComponent<Button>();
                var buttonText = buttonObject.GetComponentInChildren<TMP_Text>();
                
                buttonText.text = choiceNode.choiceText;
                
                button.onClick.AddListener(() => SelectChoice(choiceNode, director, dialogueNode));
                
                #if CLOGIC_CONDITIONALS
                button.interactable = choiceNode.conditional == null || choiceNode.conditional.Evaluate();
                #endif
                
                buttonObject.SetActive(true);
            }
        }
        
        #if CLOGIC_CONDITIONALS
        private void HandleAutoChoice(BranchNodeData dialogueNode, DialogueDirector director)
        {
            int selectedIndex = -1;
            for(int i = 0; i < dialogueNode.childBlocks.Count; i++)
            {
                DialogueNodeData blockNodeData = dialogueNode.childBlocks[i];
                var choiceNode = (ChoiceNodeData)blockNodeData;
                if(!choiceNode.conditional)
                    continue;

                if(selectedIndex != -1)
                {
                    Integrations.LogWarning($"[Dialogue] Auto choice has multiple valid branches. Choice {selectedIndex} and {i} are both valid. Defaulting to normal choice logic");
                    HandleChoice(dialogueNode, director);
                    return;
                }
                selectedIndex = i;
            }
            
            SelectChoice((ChoiceNodeData)dialogueNode.childBlocks[selectedIndex], director, dialogueNode);
        }
        #endif
        
        protected override bool CanProgressNode(BranchNodeData nodeData, DialogueDirector director) => false;
        
        private void SelectChoice(ChoiceNodeData choice, DialogueDirector director, BranchNodeData branchNodeData)
        {
            DestroyChoiceButtons();
            
            if(choice.startNodeActionID != -1)
                director.ProcessNode(director.GetNodeFromID(choice.startNodeActionID), true);
            
            branchNodeData.execOutputPortHash = choice.execOutputPortHash;
            director.GoToNode(choice.nextNodeID, true);
        }
        
        private void DestroyChoiceButtons()
        {
            foreach (Transform choiceButton in choiceContainer)
            {
                Destroy(choiceButton.gameObject);
            }
        }
        
        public override void HandleCancellation(DialogueNodeData nodeData, DialogueDirector director) => DestroyChoiceButtons();
    }
}
