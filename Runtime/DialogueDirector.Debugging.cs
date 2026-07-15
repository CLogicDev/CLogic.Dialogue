#if UNITY_EDITOR
using CLogic.Utils;
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
        
        private void ShowVisualizationForNode(DialogueNodeData nodeData, IDialogueProcessor processor)
        {
            processor.VisualizeNode(currentContext, nodeData);
            ShowExecutionPath(nodeData);
        }
        
        private void ShowExecutionPath(DialogueNodeData newNode)
        {
            Hash128 outputPort = previousNode?.execOutputPortHash ?? CurrentDialogue.DialogueGraph.entryPortHash; // Null when processing the entry node
            previousNode = newNode;
            Hash128 inputPort = newNode.execInputPortHash;
            
            if (!inputPort.isValid)
            {
                Integrations.LogWarning("Input port is not valid to show execution path");
                return;
            }
            
            if (!outputPort.isValid)
            {
                Integrations.LogWarning("Output port port is not valid to show execution path");
                return;
            }
            
            WireReference wire = currentContext.GetWireReference(outputPort, inputPort);
            
            wire.IsDashed = true;
            currentContext.Motion.Play(wire);
        }
    }
}
#endif
