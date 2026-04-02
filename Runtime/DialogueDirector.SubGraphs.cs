using System;
using System.Collections.Generic;
namespace CLogic.Systems.DialogueSystem
{
    public partial class DialogueDirector
    {
        public struct SubGraph
        {
            public DialogueGraph graph;
            public Action finishCallback;
            public int originatingID;
            
        }
        
        private Stack<SubGraph> graphStack = new();

        private void ProcessSubGraph(SubGraphNodeData subgraph)
        {
            graphStack.Push(new SubGraph()
            {
                graph = CurrentGraph,
                finishCallback = currentFinishCallback,
                originatingID = currentNodeID
            });
            
            PlayDialogueGraph(subgraph.graph, HandleSubGraphFinished);
        }

        private void HandleSubGraphFinished()
        {
            if(!graphStack.TryPop(out SubGraph graph))
                return;
            
            PlayDialogueGraph(graph.graph, graph.finishCallback, true, graph.graph.nodes[graph.originatingID].nextNodeID);
        }
    }
}
