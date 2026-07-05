using System;
using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;

namespace CLogic.Dialogue
{
    [Serializable]
    public abstract class DialogueNodeData
    {
        // Debugging References
        public Hash128 nodeHash;
        
        public Hash128 execInputPortHash;
        public Hash128 execOutputPortHash;
        
        public int nextNodeID = -1;
        
        public int startNodeActionID = -1;
        public int endNodeActionID = -1;
    }
    
    [Serializable]
    public class ContextNodeData : DialogueNodeData
    {
        [SerializeReference]
        public List<DialogueNodeData> childBlocks = new();
    }
    
    [Serializable]
    public class BlockNodeData : DialogueNodeData
    {}
    
    /// <summary>
    /// Used to refer to processor nodes in the inspector.
    /// Do not inherit from this class. Use <see cref="DialogueProcessor{T}"/> instead.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class DialogueProcessor : MonoBehaviour, IDialogueProcessor
    {
        public abstract Type NodeType { get; }
        
        public abstract bool CanProgressNode(DialogueNodeData nodeData, DialogueDirector director);
        public abstract void ProcessNode(DialogueNodeData nodeData, DialogueDirector director);
        public abstract void HandleCancellation(DialogueNodeData nodeData, DialogueDirector director);
        
        #if UNITY_EDITOR
        public virtual void VisualizeNode(Unity.GraphToolkit.Editor.GraphVisualization.Context ctx, DialogueNodeData nodeData)
        {}
        #endif
        
        [Obsolete("Use DialogueNodeProcessor<T> instead", false)]
        protected internal DialogueProcessor(){}
    }
    
    public abstract class DialogueProcessor<T> : DialogueProcessor where T : DialogueNodeData
    {
        public override Type NodeType => typeof(T);
        public sealed override bool CanProgressNode(DialogueNodeData nodeData, DialogueDirector director)
        {
            if (!CanProgressNode((T)nodeData, director))
                return false;
            
            if (nodeData.endNodeActionID != -1)
                director.ProcessNode(director.GetNodeFromID(nodeData.endNodeActionID), true);
            
            return true;
        }
        
        protected virtual bool CanProgressNode(T nodeData, DialogueDirector director) => true;
        
        #pragma warning disable CS0618
        protected DialogueProcessor()
        {}
        #pragma warning restore CS0618
        
        public sealed override void ProcessNode(DialogueNodeData nodeData, DialogueDirector director)
        {
            if (nodeData.startNodeActionID != -1)
                director.ProcessNode(director.GetNodeFromID(nodeData.startNodeActionID), true);
            
            ProcessNode((T)nodeData, director);
        }
        
        protected abstract void ProcessNode(T nodeData, DialogueDirector director);
        
        public override void HandleCancellation(DialogueNodeData nodeData, DialogueDirector director) => HandleCancellation((T)nodeData, director);
        
        protected virtual void HandleCancellation(T nodeData, DialogueDirector director) 
        {}
        
        #if UNITY_EDITOR
        public sealed override void VisualizeNode(Unity.GraphToolkit.Editor.GraphVisualization.Context ctx, DialogueNodeData nodeData) => VisualizeNode(ctx, (T)nodeData);
        protected virtual void VisualizeNode(Unity.GraphToolkit.Editor.GraphVisualization.Context ctx, T nodeData)
        {}
        #endif
    }
}
