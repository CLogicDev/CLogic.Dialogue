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
                        
            #if UNITY_EDITOR
            public Unity.GraphToolkit.Editor.GraphVisualization.Context visualizationContext;
            #endif
        }
        
        private Stack<SubGraph> graphStack = new();
        
        private void ProcessSubGraph(SubGraphNodeData subgraph)
        {
            graphStack.Push(new SubGraph
            {
                dialogue = CurrentDialogue,
                originatingID = currentNodeID,
                #if UNITY_EDITOR
                visualizationContext = currentContext
                #endif
            });
            PlayDialogueGraph(subgraph.graph, HandleSubGraphFinished, callFinishCallback: false);
        }
        
        private void HandleSubGraphFinished()
        {
            if (!graphStack.TryPop(out SubGraph graph))
                return;
            
            #if UNITY_EDITOR
            currentContext = graph.visualizationContext;
            #endif
            
            PlayDialogueGraph(graph.dialogue.DialogueGraph, graph.finishCallback, true, graph.dialogue.DialogueGraph.nodes[graph.originatingID].nextNodeID, createVisualizationContext: false);
        }
    }
}
