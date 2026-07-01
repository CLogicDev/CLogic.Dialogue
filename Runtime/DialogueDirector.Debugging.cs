using Unity.GraphToolkit.Editor.GraphVisualization;
using UnityEngine;

namespace CLogic.Dialogue
{
    public partial class DialogueDirector
    {
        private Context currentContext;
        private DialogueNodeData previousNode;
        
        private void SetupDebugContext(DialogueGraph graph)
        {
            currentContext?.Dispose();
            previousNode = null;
            currentContext = Registry.CreateVisualizationContext(graph.graphHash);
        }
        
        private void ShowExecutionPath(DialogueNodeData newNode)
        {
            Hash128 outputPort;
            if (previousNode == null)
            {
                outputPort = CurrentDialogue.DialogueGraph.entryPortHash;
                previousNode = newNode;
            }
            else
                outputPort = previousNode.execOutputPortHash;
            Hash128 inputPort = newNode.execInputPortHash;
            
            WireReference wire = currentContext.GetWireReference(outputPort, inputPort);
            wire.IsDashed = true;
        }
        
        public void ShowNodeProgress(DialogueNodeData node)
        {
        }
    }
}
