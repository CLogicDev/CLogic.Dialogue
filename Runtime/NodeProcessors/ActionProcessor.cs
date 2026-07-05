using System;
using System.Collections.Generic;
using UnityEngine;

namespace CLogic.Dialogue
{
    [Serializable]
    public class ActionNodeData : ContextNodeData
    {}
    
    public class ActionProcessor : IDialogueProcessor
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
        #if UNITY_EDITOR
        public void VisualizeNode(Unity.GraphToolkit.Editor.GraphVisualization.Context ctx, DialogueNodeData nodeData)
        {}
        #endif
    }
}
