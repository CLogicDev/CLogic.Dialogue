using System;
using UnityEngine;

namespace CLogic.Dialogue
{
    public interface IDialoguePreProcessor : IDialogueProcessorBase
    {
        public int Priority { get; }
        
        internal void PreProcessInternal(DialogueNodeData nodeData, DialogueDirector director);
    }
    
    public abstract class DialoguePreProcessor<T> : MonoBehaviour, IDialoguePreProcessor where T : DialogueNodeData
    {
        public Type NodeType => typeof(T);
        public abstract int Priority { get; }
        
        void IDialoguePreProcessor.PreProcessInternal(DialogueNodeData nodeData, DialogueDirector director) => PreProcess((T)nodeData, director);
        protected abstract void PreProcess(T nodeData, DialogueDirector director);
        
    }
}
