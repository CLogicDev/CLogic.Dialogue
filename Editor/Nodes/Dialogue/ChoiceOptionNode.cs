using System;
using System.Linq;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using CLogic.Dialogue;

namespace CLogic.Dialogue.Editor
{
    #if ENABLE_CHOICE_OPTION_NODE
    [Serializable, UseWithContext(typeof(ChoiceNode)),Node("", "", "Choice Option")]
    public class ChoiceOptionNode : DialogueBlockNode<ChoiceOptionData>
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
        
        public override ChoiceOptionData ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portMap)
        {
            ChoiceOptionData optionData = new();
            
            IDialogueGraphNode.CreateExecutionNodeLink(optionData, portMap, this);
            
            optionData.choiceText = GetPortValue<string>(GetInputPortByName(IN_TEXT));
            
            #if CLOGIC_CONDITIONALS
            optionData.conditional = GetPortValue<Conditionals.ConditionalEvaluator>(GetInputPortByName(IN_CONDITIONAL));
            #endif
            
            IDialogueGraphNode.CreateActionNodeLink(optionData, portMap, this, SupportStartAction, SupportEndAction);
            
            return optionData;
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
    #endif
}
