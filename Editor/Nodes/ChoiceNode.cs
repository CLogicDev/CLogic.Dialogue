using System;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;

namespace CLogic.Systems.DialogueSystem.Editor
{
    [Serializable, Node("Basic Nodes")]
    public class ChoiceNode : DialogueContextNode<BranchNodeData>
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<IDialogueGraphNode>("In").WithConnectorUI(PortConnectorUI.Arrowhead).WithDisplayName(string.Empty).Build();
            HandleSetup();
        }

        public override BranchNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<INode, int> nodeMap)
        {
            BranchNodeData nodeData = new();
            ProcessChildBlocks(nodeData, graph, nodeMap);

            return nodeData;
        }
        
        public override void OnValidate(GraphLogger graphLogger)
        {
            if(BlockCount == 0)
            {
                graphLogger.LogError("Branch node needs at least one branch output", this);
            }

            foreach (var block in BlockNodes)
            {
                if (block is IDialogueGraphNode dialogueNode)
                    dialogueNode.OnValidate(graphLogger);
            }
        }

        void HandleSetup()
        {
            if(BlockCount == 0)
                CreateBlockNode<ChoiceOptionNode>();
        }
    }
}
