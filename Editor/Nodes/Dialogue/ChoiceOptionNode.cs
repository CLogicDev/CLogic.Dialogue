using System;
using System.Linq;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using CLogic.Dialogue;

namespace CLogic.Dialogue.Editor
{
    [Serializable, UseWithContext(typeof(ChoiceNode)),Node("", "", "Choice Option")]
    public class ChoiceOptionNode : DialogueBlockNode<ChoiceNodeData>
    {
        private const string IN_TEXT = "Text";
        private const string OUT_EXECUTION = "Out";
        
        #if CLOGIC_CONDITIONALS
        public const string IN_CONDITIONAL = "Conditional";
        #endif
        
        public override bool SupportEndAction => false;
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<string>(IN_TEXT).Build();
            context.AddOutputPort<IDialogueGraphNode>(OUT_EXECUTION).WithConnectorUI(PortConnectorUI.Arrowhead).WithDisplayName(string.Empty).Build();
            
            base.OnDefinePorts(context);
            
            #if CLOGIC_CONDITIONALS
            context.AddInputPort<Conditionals.ConditionalEvaluator>(IN_CONDITIONAL).Build();
            #endif
        }
        
        public override ChoiceNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<INode, int> nodeMap)
        {
            ChoiceNodeData nodeData = new();
            
            IPort connectedPort = GetOutputPortByName(OUT_EXECUTION)?.FirstConnectedPort;
            
            if (connectedPort != null && nodeMap.TryGetValue(connectedPort.GetNode(), out int nodeID))
                nodeData.nextNodeID = nodeID;
            
            nodeData.choiceText = GetPortValue<string>(GetInputPortByName(IN_TEXT));
            
            #if CLOGIC_CONDITIONALS
            nodeData.conditional = GetPortValue<Conditionals.ConditionalEvaluator>(GetInputPortByName(IN_CONDITIONAL));
            #endif
            
            CreateActionNodeLink(nodeData, nodeMap);
            
            return nodeData;
        }
        
        public override void OnValidate(GraphLogger graphLogger)
        {
            base.OnValidate(graphLogger);
            
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
