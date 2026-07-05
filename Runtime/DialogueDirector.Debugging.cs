using Unity.GraphToolkit.Editor.GraphVisualization;
using UnityEngine;

namespace CLogic.Dialogue
{
    #if UNITY_EDITOR
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
        
        private void ShowVisualizationForNode(DialogueNodeData nodeData, IDialogueProcessor processor)
        {
            processor.VisualizeNode(currentContext, nodeData);
            ShowExecutionPath(nodeData);
        }
        
        private void ShowExecutionPath(DialogueNodeData newNode)
        {
            Hash128 outputPort;
            if (previousNode == null) // Occurs when processing the entry node
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
    }
    #endif
}
