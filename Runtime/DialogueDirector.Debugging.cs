#if UNITY_EDITOR
using System;
using System.Reflection;
using CLogic.Utils;
using Unity.GraphToolkit.Editor.GraphVisualization;
using UnityEngine;

namespace CLogic.Dialogue
{
    public partial class DialogueDirector
    {
        public Context CurrentContext {get; private set;}
        private DialogueNodeData previousNode;
        
        private void SetupDebugContext(DialogueGraph graph)
        {
            Debug.Log("SetupDebugContext");
            CurrentContext = Registry.CreateVisualizationContext(graph.graphHash);
        }
        
        internal bool IsContextDisposed(Context context)
        {
            FieldInfo? field = context.GetType().GetField(
                "m_Disposed",
                BindingFlags.Instance | BindingFlags.NonPublic);
            
            return field?.GetValue(context) is true;
        }
        
        private void ShowVisualizationForNode(DialogueNodeData nodeData, IDialogueProcessor processor)
        {
            processor.VisualizeNode(CurrentContext, nodeData);
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
            WireReference wire;
            
            // Wire goes into a subgraph
            if (CurrentDialogue.DialogueGraph.subgraphWireReferences.TryGetValue(inputPort, out DialogueGraph.SubgraphWireReference intoSubgraphWireRef))
            {
                WireReference parentGraphWire = CurrentContext.GetWireReference(outputPort, intoSubgraphWireRef.subgraphNodePort);
                WireReference subgraphEntryWire = CurrentContext.GetWireReference(intoSubgraphWireRef.variableNodePort, inputPort);
                
                wire = parentGraphWire;
                subgraphEntryWire.IsDashed = true;
                CurrentContext.Motion.Play(subgraphEntryWire);
            }
            // Wire comes out from a subgraph
            else if (CurrentDialogue.DialogueGraph.subgraphWireReferences.TryGetValue(outputPort, out DialogueGraph.SubgraphWireReference outOfSubgraphWireRef))
            {
                WireReference parentGraphWire = CurrentContext.GetWireReference(outOfSubgraphWireRef.subgraphNodePort, inputPort);
                WireReference subgraphEntryWire = CurrentContext.GetWireReference(outputPort, outOfSubgraphWireRef.variableNodePort);
                
                wire = parentGraphWire;
                subgraphEntryWire.IsDashed = true;
                CurrentContext.Motion.Play(subgraphEntryWire);
            }
            else
                wire = CurrentContext.GetWireReference(outputPort, inputPort);
            
            wire.IsDashed = true;
            CurrentContext.Motion.Play(wire);
        }
    }
}
#endif
