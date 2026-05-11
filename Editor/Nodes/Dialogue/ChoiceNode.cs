using System;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using System.Reflection;
using CLogic.Dialogue;

namespace CLogic.Dialogue.Editor
{
    [Serializable, Node("Basic Nodes", "", "Choice Block")]
    public class ChoiceNode : DialogueContextNode<BranchNodeData>
    {
        public override bool SupportEndAction => false;
        
        private const string OP_AUTO_CHOICE = "AutoChoice";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            #if CLOGIC_CONDITIONALS
            context.AddOption<bool>(OP_AUTO_CHOICE).WithDisplayName("Is Auto Choice").Build();
            #endif
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            var inputPort = context.AddInputPort<IDialogueGraphNode>("In").WithConnectorUI(PortConnectorUI.Arrowhead).WithDisplayName(string.Empty).Build();
            
            PropertyInfo propertyInfo = inputPort.GetType().GetProperty("Capacity", BindingFlags.Instance | BindingFlags.Public);
            Type portCapacityType = propertyInfo.PropertyType;
            object multiCapacity = Enum.Parse(portCapacityType, "Multi");
            propertyInfo.SetValue(inputPort, multiCapacity);
            
            HandleSetup();
        }
        
        public override BranchNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<INode, int> nodeMap)
        {
            BranchNodeData nodeData = new();
            ProcessChildBlocks(nodeData, graph, nodeMap);
            
            #if CLOGIC_CONDITIONALS
            GetNodeOptionByName(OP_AUTO_CHOICE).TryGetValue(out bool isAutoChoice);
            
            nodeData.isAutoChoice = isAutoChoice;
            #endif
            
            return nodeData;
        }
        
        public override void OnValidate(GraphLogger graphLogger)
        {
            base.OnValidate(graphLogger);
            
            if (BlockCount == 0)
            {
                graphLogger.LogError("Choice node needs at least one branch output", this);
            }
            
            #if CLOGIC_CONDITIONALS
            GetNodeOptionByName(OP_AUTO_CHOICE).TryGetValue(out bool isAutoChoice);
            
            if(!isAutoChoice)
                return;
            
            if(BlockCount == 1)
                return;
            
            bool hasConditionalSet = false;
            
            List<ChoiceOptionNode> emptyConditionalNode = new();
            
            foreach (BlockNode blockNode in BlockNodes)
            {
                if(blockNode is not ChoiceOptionNode choiceNode)
                    continue;

                if(IDialogueGraphNode.TryGetPortValue<Conditionals.ConditionalEvaluator>(choiceNode.GetInputPortByName(ChoiceOptionNode.IN_CONDITIONAL), out _))
                {
                    hasConditionalSet = true;
                }
                else
                {
                    emptyConditionalNode.Add(choiceNode);
                }
            }

            if(!hasConditionalSet)
            {
                graphLogger.LogError("Auto choice behaviour needs at least one conditional set to function. Defaulting to normal choice logic", this);
            }
            else
            {
                foreach (ChoiceOptionNode choiceNode in emptyConditionalNode)
                {
                    graphLogger.LogWarning("An empty conditional in an auto choice block will be considered to always return false", choiceNode);
                }
            }
            #endif
        }
        
        private void HandleSetup()
        {
            if (BlockCount == 0)
                CreateBlockNode<ChoiceOptionNode>();
        }
    }
}
