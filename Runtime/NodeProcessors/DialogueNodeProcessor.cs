using System;
using UnityEngine;
using System.Collections.Generic;

namespace CLogic.Dialogue
{
    [Serializable]
    public abstract class DialogueNodeData
    {
        public int nextNodeID = -1;
        
        public int startNodeActionID = -1;
        public int endNodeActionID = -1;
    }
    
    [Serializable]
    public class ContextNodeData : DialogueNodeData
    {
        [SerializeReference]
        public List<BlockNodeData> childBlocks = new();
    }
    
    [Serializable]
    public class BlockNodeData : DialogueNodeData
    {}
    
    /// <summary>
    /// Used to refer to processor nodes in the inspector.
    /// Do not inherit from this class. Use <see cref="DialogueNodeProcessor&lt;T&gt;"/> instead.
    /// </summary>
    public abstract class DialogueNodeProcessor : MonoBehaviour, IDialogueNodeProcessor
    {
        public abstract Type NodeType { get; }
        
        public abstract bool CanProgressNode(DialogueNodeData dialogueNode, DialogueDirector director);
        public abstract void ProcessNode(DialogueNodeData dialogueNode, DialogueDirector director);
        public abstract void HandleCancellation(DialogueNodeData dialogueNode, DialogueDirector director);
    }
    
    public abstract class DialogueNodeProcessor<T> : DialogueNodeProcessor where T : DialogueNodeData
    {
        public override Type NodeType => typeof(T);
        public override bool CanProgressNode(DialogueNodeData dialogueNode, DialogueDirector director)
        {
            if (dialogueNode.endNodeActionID != -1)
                director.ProcessNode(director.GetNodeFromID(dialogueNode.endNodeActionID), true);
            
            return true;
        }
        public sealed override void ProcessNode(DialogueNodeData dialogueNode, DialogueDirector director) => ProcessNode((T)dialogueNode, director);
        
        public virtual void ProcessNode(T dialogueNode, DialogueDirector director)
        {
            if (dialogueNode.startNodeActionID != -1)
                director.ProcessNode(director.GetNodeFromID(dialogueNode.startNodeActionID), true);
        }
        
        public override void HandleCancellation(DialogueNodeData dialogueNode, DialogueDirector director) {}
    }
}
