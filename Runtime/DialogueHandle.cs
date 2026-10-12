using System;
using System.Collections.Generic;

namespace CLogic.Dialogue
{
    internal struct SubGraph
    {
        public DialogueGraph dialogueGraph;
        public int subgraphNodeID;

#if UNITY_EDITOR
        public Unity.GraphToolkit.Editor.GraphVisualization.Context visualizationContext;
#endif
    }

    /// <summary>
    /// Represents a handle to a dialogue session, giving access to its state.
    /// </summary>
    [Serializable]
    public class DialogueHandle
    {
        private DialogueDirector director;
        internal Stack<SubGraph> executionFrames;

        public bool IsPlaying { get; internal set; }
        public bool IsFinished { get; internal set; }

        /// <summary>
        /// Gets the main dialogue graph associated with this handle.
        /// </summary>
        public DialogueGraph MainGraph { get; }

        /// <summary>
        /// Gets the current dialogue graph or subgraph being executed in this handle.
        /// </summary>
        public DialogueGraph CurrentGraph { get; internal set; }

        public event Action OnFinish;

        internal DialogueHandle()
        {
            CurrentGraph = null;
            MainGraph = null;
            IsPlaying = false;
        }

        internal DialogueHandle(DialogueDirector director, bool playState, DialogueGraph currentGraph, Action onFinish)
        {
            this.director = director;
            IsPlaying = playState;
            MainGraph = currentGraph;
            CurrentGraph = currentGraph;
            IsFinished = false;
            OnFinish = onFinish;
        }

        internal void SetDialogueFinished()
        {
            IsFinished = true;
            IsPlaying = false;
            OnFinish?.Invoke();
        }

        public void Cancel()
        {
            director.EndDialogue();
        }
    }
}
