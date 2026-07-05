using System;
namespace CLogic.Dialogue
{
    public interface IDialogueProcessor
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
        
        #if UNITY_EDITOR
        /// <summary>
        /// Called when the node is being visualized. This is used to show debugging visuals in the graph editor
        /// </summary>
        public void VisualizeNode(Unity.GraphToolkit.Editor.GraphVisualization.Context ctx, DialogueNodeData nodeData);
        #endif
    }
}
