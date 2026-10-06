using System;
using UnityEngine;
namespace CLogic.Dialogue
{
    public interface IDialoguePostProcessor : IDialogueProcessorBase
    {
        public int Priority { get; }

        internal void PostProcessInternal(DialogueNodeData nodeData, DialogueDirector director);
    }

    /// <summary>
    /// A base class for creating dialogue post-processors that can access dialogue node data after it has been processed by the dialogue system.
    /// </summary>
    /// <typeparam name="T">The type of dialogue node data this post-processor handles.</typeparam>
    public abstract class DialoguePostProcessor<T> : MonoBehaviour, IDialoguePostProcessor where T : DialogueNodeData
    {
        public Type NodeType => typeof(T);

        /// <summary>
        /// Represents the execution priority of the post-processor. Lowest is executed first.
        /// </summary>
        public abstract int Priority { get; }

        void IDialoguePostProcessor.PostProcessInternal(DialogueNodeData nodeData, DialogueDirector director) => PostProcess((T)nodeData, director);
        protected abstract void PostProcess(T nodeData, DialogueDirector director);
    }
}
