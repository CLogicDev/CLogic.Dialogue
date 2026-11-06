#if UNITY_EDITOR
using UnityEngine;
using CLogic.Utils;
using System.Reflection;
using Unity.GraphToolkit.Editor.GraphVisualization;

namespace CLogic.Dialogue
{
    public partial class DialogueDirector
    {
        private DialogueNodeData previousNode;

        public Context CurrentContext { get; private set; }

        private void SetupDebugContext(DialogueGraph graph) => CurrentContext = Registry.CreateVisualizationContext(graph.graphHash);

#nullable enable
        internal bool IsContextDisposed(Context context)
        {
            FieldInfo? field = context.GetType().GetField("m_Disposed", BindingFlags.Instance | BindingFlags.NonPublic);

            return field?.GetValue(context) is true;
        }
#nullable disable

        private void ShowVisualizationForNode(DialogueNodeData nodeData, IDialogueProcessor processor)
        {
            processor.VisualizeNode(CurrentContext, nodeData);
            ShowExecutionPath(nodeData);
        }

        private void ShowExecutionPath(DialogueNodeData newNode)
        {
            Hash128 outputPort = previousNode?.execOutputPortHash ?? CurrentDialogue.CurrentGraph.entryPortHash; // Null when processing the entry node
            Hash128 inputPort = newNode.execInputPortHash;

            previousNode = newNode;

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
            if (CurrentDialogue.CurrentGraph.subgraphWireReferences.TryGetValue(inputPort, out DialogueGraph.SubgraphWireReference intoSubgraphWireRef))
            {
                WireReference parentGraphWire = CurrentContext.GetWireReference(outputPort, intoSubgraphWireRef.subgraphNodePort);
                WireReference subgraphEntryWire = CurrentContext.GetWireReference(intoSubgraphWireRef.variableNodePort, inputPort);

                wire = parentGraphWire;
                subgraphEntryWire.IsDashed = true;
                CurrentContext.Motion.Play(subgraphEntryWire);
            }
            else if (CurrentDialogue.CurrentGraph.subgraphWireReferences.TryGetValue(outputPort, out DialogueGraph.SubgraphWireReference outOfSubgraphWireRef)) // Wire comes out from a subgraph
            {
                WireReference parentGraphWire = CurrentContext.GetWireReference(outOfSubgraphWireRef.subgraphNodePort, inputPort);
                WireReference subgraphEntryWire = CurrentContext.GetWireReference(outputPort, outOfSubgraphWireRef.variableNodePort);

                wire = parentGraphWire;
                subgraphEntryWire.IsDashed = true;
                CurrentContext.Motion.Play(subgraphEntryWire);
            }
            else
            {
                wire = CurrentContext.GetWireReference(outputPort, inputPort);
            }

            wire.IsDashed = true;
            CurrentContext.Motion.Play(wire);
        }
    }
}
#endif
