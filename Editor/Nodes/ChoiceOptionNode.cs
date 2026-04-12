using System;
using System.Linq;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;

namespace CLogic.Systems.DialogueSystem.Editor
{
    [Serializable, UseWithContext(typeof(ChoiceNode))]
    public class ChoiceOptionNode : DialogueBlockNode<ChoiceNodeData>
    {
        private const string IN_TEXT = "Text";
        private const string OUT_EXECUTION = "Out";
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<string>(IN_TEXT).Build();
            context.AddOutputPort<IDialogueGraphNode>(OUT_EXECUTION).WithConnectorUI(PortConnectorUI.Arrowhead).WithDisplayName(string.Empty).Build();
        }
        
        public override ChoiceNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<INode, int> nodeMap)
        {
            ChoiceNodeData nodeData = new();
            
            IPort connectedPort = GetOutputPorts().FirstOrDefault((port) => port.Name == OUT_EXECUTION)?.FirstConnectedPort;
            
            if (connectedPort != null && nodeMap.TryGetValue(connectedPort.GetNode(), out int nodeID))
                nodeData.nextNodeID = nodeID;
            
            nodeData.choiceText = GetPortValue<string>(GetInputPortByName(IN_TEXT));
            
            return nodeData;
        }
        
        public override void OnValidate(GraphLogger graphLogger)
        {
            List<IPort> connectedPorts = new();
            
            GetOutputPortByName(OUT_EXECUTION).GetConnectedPorts(connectedPorts);
            
            switch (connectedPorts.Count)
            {
                case 0:
                    graphLogger.Log("Node output not connected, the graph will end by default", this);
                break;
                
                case > 1:
                    graphLogger.LogError("Multiple execution output links are not allowed", this);
                break;
            }
        }
    }
}
