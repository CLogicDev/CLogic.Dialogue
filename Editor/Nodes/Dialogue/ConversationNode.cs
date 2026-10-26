using System;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;

namespace CLogic.Dialogue.Editor
{
#if !DISABLE_CONVERSATION_NODE
    [Serializable, Node("Basic Nodes", "Packages/dev.clogic.dialogue/EditorResources/Textures/Icons/Conversation Node.svg", "Conversation")]
    public class ConversationNode : DialogueNode<ConversationNodeData>
    {
        public const string IN_CHARACTER_NAME = "Character";
        public const string IN_SKIPPABLE = "Skippable";
        public const string IN_TEXT_SPEED = "Text Speed";

        public const string OP_TEXT_AREA_LINES = "Text Area Lines";

        public override void OnEnable() => Tooltip = "Defines a conversation to be displayed";

        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);

            context.AddOption<int>(OP_TEXT_AREA_LINES).WithDefaultValue(3).WithDisplayName("Text Area Width").ShowInInspectorOnly().Build();
        }

        protected override void DefineDialoguePorts(IPortDefinitionContext context)
        {
            const string TextTooltip = "The text to be displayed in the conversation";
            const string CharacterNameTooltip = "The name of the speaking character (optional)";
            const string TextSpeedTooltip = "The speed at which the text appears in words per minute. If 0 it will use the speed defined in the type writer";
            const string SkippableTooltip = "Whether the conversation can be skipped via input";

            CreateDefaultExecutionPorts(context);
            GetNodeOptionByName(OP_TEXT_AREA_LINES).TryGetValue(out int textAreaLines);

            context.AddInputPort<string>(ConversationNodeData.IN_TEXT).AsTextArea(textAreaLines).WithTooltip(TextTooltip).Build();
            context.AddInputPort<string>(IN_CHARACTER_NAME).WithTooltip(CharacterNameTooltip).Build();
            context.AddInputPort<float>(IN_TEXT_SPEED).WithTooltip(TextSpeedTooltip).Build();
            context.AddInputPort<bool>(IN_SKIPPABLE).WithDefaultValue(true).WithTooltip(SkippableTooltip).Build();
        }

        public override ConversationNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portMap)
        {
            ConversationNodeData node = new();

            CreateNodeLink(node, portMap);

            node.text = GetPortValue<string>(GetInputPortByName(ConversationNodeData.IN_TEXT));
            node.characterName = GetPortValue<string>(GetInputPortByName(IN_CHARACTER_NAME));
            node.textSpeed = GetPortValue<float>(GetInputPortByName(IN_TEXT_SPEED));
            node.skippable = GetPortValue<bool>(GetInputPortByName(IN_SKIPPABLE));

            return node;
        }
    }

    public class ConversationNodeView : DialogueNodeView<ConversationNode> { }
#endif
}
