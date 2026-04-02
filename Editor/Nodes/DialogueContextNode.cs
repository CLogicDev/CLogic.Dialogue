using System;
using System.Linq;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

namespace CLogic.Systems.DialogueSystem.Editor
{
    [Serializable]
    public abstract class DialogueContextNode<T> : ContextNode, IDialogueGraphNode where T : ContextNodeData
    {
        public const string IN_EXECUTION = "In";
        public const string OUT_EXECUTION = "Out";

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<IDialogueGraphNode>(IN_EXECUTION).WithDisplayName(string.Empty).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            context.AddOutputPort<IDialogueGraphNode>(OUT_EXECUTION).WithDisplayName(string.Empty).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
        }

        public virtual void OnValidate(GraphLogger graphLogger)
        {
            List<IPort> connectedPorts = new();

            GetOutputPortByName(OUT_EXECUTION)?.GetConnectedPorts(connectedPorts);

            switch (connectedPorts.Count)
            {
                case 0:
                    graphLogger.Log("Node output not connected, the graph will end by default", this);
                    break;

                case > 1:
                    graphLogger.LogError("Multiple execution output links are not allowed", this);
                    break;
            }

            foreach (var block in BlockNodes)
            {
                if (block is IDialogueGraphNode dialogueNode)
                    dialogueNode.OnValidate(graphLogger);
            }
        }

        public abstract T ProcessNodeAsset(DialogueGraph graph, Dictionary<INode, int> nodeMap);

        protected void ProcessChildBlocks(T nodeData, DialogueGraph graph, Dictionary<INode, int> nodeMap)
        {
            foreach (var block in BlockNodes)
            {
                if (block is IDialogueGraphNode dialogueNode)
                {
                    var blockNodeData = dialogueNode.ProcessNode(graph, nodeMap) as BlockNodeData;
                    nodeData.childBlocks.Add(blockNodeData);
                }
            }
        }

        //TODO: Find a way to remove code dupe from DialogueNode.cs
        public virtual void CreateNodeLink(T node, Dictionary<INode, int> nodeMap)
        {
            IPort connectedPort = GetOutputPorts().FirstOrDefault((port) => port.Name == OUT_EXECUTION)?.FirstConnectedPort;
            
            if(connectedPort == null)
                return;

            INode connectedNode = connectedPort.GetNode();
            if(connectedNode is EndNode)
                node.nextNodeID = -2; // Graceful end
            
            node.nextNodeID = nodeMap.GetValueOrDefault(connectedNode, -1);
        }

        DialogueNodeData IDialogueGraphNode.ProcessNode(DialogueGraph graph, Dictionary<INode, int> nodeMap) => ProcessNodeAsset(graph, nodeMap);
    }
}
