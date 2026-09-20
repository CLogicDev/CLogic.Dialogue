using System;
using System.Collections;
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
    
    [Serializable]
    public class DialogueHandle
    {
        private DialogueDirector director;

        public DialogueGraph MainGraph { get; }
        public DialogueGraph CurrentGraph { get; internal set; }
        public bool IsPlaying { get; internal set; }
        public bool IsFinished { get; internal set; }

        public event Action OnFinish;
        
        internal Stack<SubGraph> executionFrames;

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
