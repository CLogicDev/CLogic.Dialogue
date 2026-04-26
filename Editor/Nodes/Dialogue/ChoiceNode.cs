using System;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using CLogic.Dialogue;

namespace CLogic.Dialogue.Editor
{
    [Serializable, Node("Basic Nodes")]
    public class ChoiceNode : DialogueContextNode<BranchNodeData>
    {
        public override bool SupportExecution => false;
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
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
            base.OnValidate(graphLogger);
            
            if (BlockCount == 0)
            {
                graphLogger.LogError("Choice node needs at least one branch output", this);
            }
        }
        
        private void HandleSetup()
        {
            if (BlockCount == 0)
                CreateBlockNode<ChoiceOptionNode>();
        }
    }
}
