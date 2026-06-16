using System;
namespace CLogic.Dialogue
{
    public interface IDialogueNodeProcessor
    {
        public Type NodeType { get; }
        
        /// <summary>
        /// Called when the director requests to move to the next node
        /// </summary>
        /// <returns>Whether the dialogue director can progress to the next node</returns>
        public bool CanProgressNode(DialogueNodeData nodeData, DialogueDirector director);
        
        public void ProcessNode(DialogueNodeData nodeData, DialogueDirector director);
        
        /// <summary>
        /// Called when the current graph is cancelled which this processor was active
        /// </summary>
        public void HandleCancellation(DialogueNodeData nodeData, DialogueDirector director);
    }
}
