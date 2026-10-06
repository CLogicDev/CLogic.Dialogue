using System;
using UnityEngine;

namespace CLogic.Dialogue
{
    public interface IDialoguePreProcessor : IDialogueProcessorBase
    {
        public int Priority { get; }

        internal void PreProcessInternal(DialogueNodeData nodeData, DialogueDirector director);
    }

    /// <summary>
    /// A base class for creating dialogue pre-processors that can access dialogue node data before it is processed by the dialogue system.
    /// </summary>
    /// <typeparam name="T">The type of dialogue node data this pre-processor handles.</typeparam>
    public abstract class DialoguePreProcessor<T> : MonoBehaviour, IDialoguePreProcessor where T : DialogueNodeData
    {
        public Type NodeType => typeof(T);

        /// <summary>
        /// Represents the execution priority of the pre-processor. Lowest is executed first.
        /// </summary>
        public abstract int Priority { get; }

        void IDialoguePreProcessor.PreProcessInternal(DialogueNodeData nodeData, DialogueDirector director) => PreProcess((T)nodeData, director);
        protected abstract void PreProcess(T nodeData, DialogueDirector director);
    }
}
