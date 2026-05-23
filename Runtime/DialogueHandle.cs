using System;
namespace CLogic.Dialogue
{
    [Serializable]
    public class DialogueHandle
    {
        private DialogueDirector director;

        public DialogueGraph DialogueGraph { get; }
        public bool IsPlaying { get; internal set; }
        public bool IsFinished { get; internal set; }

        public event Action OnFinish;

        internal DialogueHandle()
        {
            DialogueGraph = null;
            IsPlaying = false;
        }
        
        internal DialogueHandle(DialogueDirector director, bool playState, DialogueGraph dialogueGraph, Action onFinish)
        {
            this.director = director;
            IsPlaying = playState;
            DialogueGraph = dialogueGraph;
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
