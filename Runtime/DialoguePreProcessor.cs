using System;
using UnityEngine;
namespace CLogic.Dialogue
{
    public interface IDialoguePreProcessor
    {
        public Type HandledType { get; }
        
        public int Priority { get; }
        
        public void PreProcess(DialogueNodeData nodeData, DialogueDirector director);
    }
    
    public abstract class DialoguePreProcessor<T> : MonoBehaviour, IDialogueProcessor, IDialoguePreProcessor where T : DialogueNodeData
    {
        public Type HandledType => typeof(T);
        public abstract int Priority { get; }
        
        public void PreProcess(DialogueNodeData nodeData, DialogueDirector director) => PreProcess((T)nodeData, director);
        
        protected abstract void PreProcess(T nodeData, DialogueDirector director);
        
        #region Interface Contracts
        public Type NodeType => HandledType;
        public bool CanProgressNode(DialogueNodeData nodeData, DialogueDirector director) => throw new Exception($"{nameof(DialoguePreProcessor<T>)} cannot process nodes");
        public void ProcessNode(DialogueNodeData nodeData, DialogueDirector director) => throw new Exception($"{nameof(DialoguePreProcessor<T>)} cannot process nodes");
        public void HandleCancellation(DialogueNodeData nodeData, DialogueDirector director) => throw new Exception($"{nameof(DialoguePreProcessor<T>)} cannot process nodes");
        #if UNITY_EDITOR
        public void VisualizeNode(Unity.GraphToolkit.Editor.GraphVisualization.Context ctx, DialogueNodeData nodeData) => throw new Exception($"{nameof(DialoguePreProcessor<T>)} cannot process nodes");
        #endif
        #endregion
    }
}
