using System;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using CLogic.Dialogue;

namespace CLogic.Dialogue.Editor
{
    #if ENABLE_CONVERSATION_NODE
    
    [Serializable, Node("Basic Nodes", "", "Conversation")]
    public class ConversationNode : DialogueNode<ConversationNodeData>
    {
        public const string IN_TEXT = "Text";
        public const string IN_CHARACTER_NAME = "Character";
        public const string IN_SKIPPABLE = "Skippable";
        public const string IN_TEXT_SPEED = "Text Speed";
        
        public const string OP_TEXT_AREA_LINES = "Text Area Lines";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<int>(OP_TEXT_AREA_LINES).WithDefaultValue(3).WithDisplayName("Text Area Width").ShowInInspectorOnly().Build();
        }
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            CreateDefaultExecutionPorts(context);
            
            GetNodeOptionByName(OP_TEXT_AREA_LINES).TryGetValue(out int textAreaLines);
            context.AddInputPort<string>(IN_TEXT).AsTextArea(textAreaLines).Build();
            
            context.AddInputPort<string>(IN_CHARACTER_NAME).Build();
            context.AddInputPort<float>(IN_TEXT_SPEED).Build();
            context.AddInputPort<bool>(IN_SKIPPABLE).WithDefaultValue(true).Build();
        }
        
        public override ConversationNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<INode, int> nodeMap)
        {
            ConversationNodeData node = new();
            
            CreateNodeLink(node, nodeMap);
            
            node.text = GetPortValue<string>(GetInputPortByName(IN_TEXT));
            node.characterName = GetPortValue<string>(GetInputPortByName(IN_CHARACTER_NAME));
            node.textSpeed = GetPortValue<float>(GetInputPortByName(IN_TEXT_SPEED));
            node.skippable = GetPortValue<bool>(GetInputPortByName(IN_SKIPPABLE));
            
            return node;
        }
    }
    #endif
}
