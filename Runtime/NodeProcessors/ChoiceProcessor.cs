using TMPro;
using System;
using CLogic.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace CLogic.Dialogue
{
    [Serializable]
    public class ChoiceNodeData : ContextNodeData
    {
        #if CLOGIC_CONDITIONALS
        public bool isAutoChoice;
        #endif
    }
    
    [Serializable]
    public class ChoiceOptionData : BlockNodeData
    {
        public string choiceText;
        
        #if CLOGIC_CONDITIONALS
        public Conditional.ConditionalEvaluator conditional;
        #endif
        
    }
    
    public class ChoiceProcessor : DialogueProcessor<ChoiceNodeData>
    {
        [SerializeField]
        private GameObject choiceButtonPrefab;
        [SerializeField]
        private RectTransform choiceContainer;
        
        protected override void ProcessNode(ChoiceNodeData nodeData, DialogueDirector director)
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

        private void HandleChoice(ChoiceNodeData dialogueNode, DialogueDirector director)
        {
            foreach (DialogueNodeData dialogueNodeData in dialogueNode.childBlocks)
            {
                var choiceNode = dialogueNodeData as ChoiceOptionData;
                
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
        private void HandleAutoChoice(ChoiceNodeData dialogueNode, DialogueDirector director)
        {
            int selectedIndex = -1;
            for(int i = 0; i < dialogueNode.childBlocks.Count; i++)
            {
                DialogueNodeData blockNodeData = dialogueNode.childBlocks[i];
                var choiceNode = (ChoiceOptionData)blockNodeData;
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
            
            SelectChoice((ChoiceOptionData)dialogueNode.childBlocks[selectedIndex], director, dialogueNode);
        }
        #endif
        
        protected override bool CanProgressNode(ChoiceNodeData nodeData, DialogueDirector director) => false;
        
        private void SelectChoice(ChoiceOptionData choice, DialogueDirector director, ChoiceNodeData choiceNodeData)
        {
            DestroyChoiceButtons();
            
            if(choice.startNodeActionID != -1)
                director.ProcessNode(director.GetNodeFromID(choice.startNodeActionID), true);
            
            choiceNodeData.execOutputPortHash = choice.execOutputPortHash;
            director.GoToNode(choice.nextNodeID, true);
        }
        
        private void DestroyChoiceButtons()
        {
            foreach (Transform choiceButton in choiceContainer)
            {
                Destroy(choiceButton.gameObject);
            }
        }
        
        protected override void HandleCancellation(ChoiceNodeData nodeData, DialogueDirector director) => DestroyChoiceButtons();
    }
}
