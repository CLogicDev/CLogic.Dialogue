using System;
using System.Collections.Generic;
namespace CLogic.Dialogue
{
    public partial class DialogueDirector
    {
        public bool IsInSubGraph => CurrentDialogue?.executionFrames?.Count > 0;
        
        
        private void ProcessSubGraph(SubGraphNodeData subgraph)
        {
            CurrentDialogue.executionFrames ??= new Stack<SubGraph>();
            
            CurrentDialogue.executionFrames.Push(new SubGraph
            {
                dialogueGraph = CurrentDialogue.CurrentGraph,
                subgraphNodeID = currentNodeID,
                
                #if UNITY_EDITOR
                visualizationContext = CurrentContext
                #endif
            });
            
            CurrentDialogue.CurrentGraph = subgraph.graph;
            CurrentNode = null;
            LoadGraph(subgraph.graph);
            
            GoToNode(subgraph.graph.startNodeID, true);
        }
        
        
        private void HandleSubGraphFinished()
        {
            if (!CurrentDialogue.executionFrames.TryPop(out SubGraph subgraph))
                throw new Exception("Subgraph did not terminate properly");
            
            CurrentDialogue.CurrentGraph = subgraph.dialogueGraph;
            LoadGraph(subgraph.dialogueGraph, false);
            
            #if UNITY_EDITOR
            CurrentContext = subgraph.visualizationContext;
            #endif
            
            var subgraphNode = nodes[subgraph.subgraphNodeID] as SubGraphNodeData;
            CurrentNode = null;
            GoToNode(subgraphNode.nextNodeID, true);
        }
    }
}
