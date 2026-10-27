using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;

namespace CLogic.Dialogue.Editor
{
    [Serializable, Node("Basic", "Packages/dev.clogic.dialogue/EditorResources/Textures/Icons/StartNode.svg", "Start Point")]
    public class StartNode : Node, IDialogueGraphNode, IConnectionValidator
    {
        public const string OUT_START = "Entry";

        public override void OnEnable()
        {
            DefaultColor = Color.limeGreen;
            Tooltip = "The entry point of the graph";
        }

        protected override void OnDefinePorts(IPortDefinitionContext context) => context.AddOutputPort<IDialogueGraphNode>(OUT_START).WithConnectorUI(PortConnectorUI.Arrowhead).Build();

        public void OnValidate(GraphLogger graphLogger)
        {
            List<IPort> connectedPorts = new();
            GetOutputPortByName(OUT_START).GetConnectedPorts(connectedPorts);

            switch (connectedPorts.Count)
            {
                case > 1:
                    graphLogger.LogError("Multiple execution output links are not allowed", this);
                    break;

                case 0:
                    graphLogger.LogWarning("Start node not connected, make sure you have a connected entry point in your graph.", this);
                    break;
            }
        }

        DialogueNodeData IDialogueGraphNode.ProcessNode(DialogueGraph graph, Dictionary<IPort, int> portMap)
        {
            IPort port = GetOutputPortByName(OUT_START).FirstConnectedPort;

            if (port == null)
                return null;

            graph.startNodeID = portMap[port];
            graph.entryPortHash = GetOutputPortByName(OUT_START).ID;

            return null;
        }

        public bool? CanConnect(IPort output, IPort input) => input.GetNode() is not ActionNode && input.Name is IDialogueGraphNode.IN_EXECUTION;
    }
}
