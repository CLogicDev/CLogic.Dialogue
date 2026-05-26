using System;
using System.Collections.Generic;
namespace CLogic.Dialogue
{
    public partial class DialogueDirector
    {
        public struct SubGraph
        {
            public DialogueHandle dialogue;
            public Action finishCallback;
            public int originatingID;
        }
        
        private Stack<SubGraph> graphStack = new();
        
        private void ProcessSubGraph(SubGraphNodeData subgraph)
        {
            var handle = PlayDialogueGraph(subgraph.graph, HandleSubGraphFinished);
            graphStack.Push(new SubGraph
            {
                dialogue = handle,
                originatingID = currentNodeID
            });
        }
        
        private void HandleSubGraphFinished()
        {
            if (!graphStack.TryPop(out SubGraph graph))
                return;
            
            PlayDialogueGraph(graph.dialogue.DialogueGraph, graph.finishCallback, true, graph.dialogue.DialogueGraph.nodes[graph.originatingID].nextNodeID);
        }
    }
}
