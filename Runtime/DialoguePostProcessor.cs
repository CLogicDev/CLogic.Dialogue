using System;
using UnityEngine;
namespace CLogic.Dialogue
{
    public interface IDialoguePostProcessor : IDialogueProcessorBase
    {
        public int Priority { get; }
        
        internal void PostProcessInternal(DialogueNodeData nodeData, DialogueDirector director);
    }
    
    public abstract class DialoguePostProcessor<T> : MonoBehaviour, IDialoguePostProcessor where T : DialogueNodeData
    {
        public Type NodeType => typeof(T);
        public abstract int Priority { get; }
        
        void IDialoguePostProcessor.PostProcessInternal(DialogueNodeData nodeData, DialogueDirector director) => PostProcess((T)nodeData, director);
        protected abstract void PostProcess(T nodeData, DialogueDirector director);
    }
}
