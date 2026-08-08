using System;
using UnityEngine;
namespace CLogic.Dialogue
{
    public interface IDialoguePostProcessor
    {
        public Type HandledType { get; }
        
        public int Priority { get; }
        
        internal void PostProcessInternal(DialogueNodeData nodeData, DialogueDirector director);
    }
    
    public abstract class DialoguePostProcessor<T> : MonoBehaviour, IDialogueProcessor, IDialoguePostProcessor where T : DialogueNodeData
    {
        public Type HandledType => typeof(T);
        public abstract int Priority { get; }
        
        void IDialoguePostProcessor.PostProcessInternal(DialogueNodeData nodeData, DialogueDirector director) => PostProcess((T)nodeData, director);
        protected abstract void PostProcess(T nodeData, DialogueDirector director);
        
        #region Interface Contracts
        public Type NodeType => HandledType;
        public bool CanProgressNode(DialogueNodeData nodeData, DialogueDirector director) => throw new Exception($"{nameof(DialoguePostProcessor<T>)} cannot process nodes");
        public void ProcessNode(DialogueNodeData nodeData, DialogueDirector director) => throw new Exception($"{nameof(DialoguePostProcessor<T>)} cannot process nodes");
        public void HandleCancellation(DialogueNodeData nodeData, DialogueDirector director) => throw new Exception($"{nameof(DialoguePostProcessor<T>)} cannot process nodes");
        #if UNITY_EDITOR
        public void VisualizeNode(Unity.GraphToolkit.Editor.GraphVisualization.Context ctx, DialogueNodeData nodeData) => throw new Exception($"{nameof(DialoguePostProcessor<T>)} cannot process nodes");
        #endif
        #endregion
    }
}
