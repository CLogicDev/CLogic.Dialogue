using System;
using System.Collections.Generic;
using CLogic.Dialogue;
using Unity.GraphToolkit.Editor;

namespace CLogic.Dialogue.Editor
{
    [Serializable, Node("Basic Nodes", "", "Start Point")]
    public class StartNode : Node, IDialogueGraphNode
    {
        public const string OUT_START = "Start";
        
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
        
        DialogueNodeData IDialogueGraphNode.ProcessNode(DialogueGraph graph, Dictionary<INode, int> nodeMap)
        {
            IPort port = GetOutputPortByName(OUT_START).FirstConnectedPort;
            
            if (port == null)
                return null;
            
            graph.startNodeID = nodeMap[port.GetNode()];
            
            return null;
        }
    }
}
