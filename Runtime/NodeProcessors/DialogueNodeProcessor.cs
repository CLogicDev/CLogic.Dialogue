using System;
using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;

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
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class DialogueNodeProcessor : MonoBehaviour, IDialogueNodeProcessor
    {
        public abstract Type NodeType { get; }
        
        public abstract bool CanProgressNode(DialogueNodeData dialogueNode, DialogueDirector director);
        public abstract void ProcessNode(DialogueNodeData nodeDate, DialogueDirector director);
        public abstract void HandleCancellation(DialogueNodeData nodeDate, DialogueDirector director);
        
        [Obsolete("Use DialogueNodeProcessor<> instead", false)]
        protected internal DialogueNodeProcessor(){}
    }
    
    public abstract class DialogueNodeProcessor<T> : DialogueNodeProcessor where T : DialogueNodeData
    {
        public override Type NodeType => typeof(T);
        public sealed override bool CanProgressNode(DialogueNodeData nodeDate, DialogueDirector director) => CanProgressNode((T)nodeDate, director);
        protected virtual bool CanProgressNode(T nodeData, DialogueDirector director)
        {
            if (nodeData.endNodeActionID != -1)
                director.ProcessNode(director.GetNodeFromID(nodeData.endNodeActionID), true);
            
            return true;
        }
        
        #pragma warning disable CS0618
        protected DialogueNodeProcessor()
        {}
        #pragma warning restore CS0618
        
        public sealed override void ProcessNode(DialogueNodeData nodeDate, DialogueDirector director) => ProcessNode((T)nodeDate, director);
        
        protected virtual void ProcessNode(T nodeData, DialogueDirector director)
        {
            if (nodeData.startNodeActionID != -1)
                director.ProcessNode(director.GetNodeFromID(nodeData.startNodeActionID), true);
        }
        
        public override void HandleCancellation(DialogueNodeData nodeDate, DialogueDirector director) => HandleCancellation((T)nodeDate, director);
        
        protected virtual void HandleCancellation(T nodeDate, DialogueDirector director) { }
    }
}
