using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;

namespace CLogic.Dialogue.Editor
{
    [Serializable, Node("Basic", "Packages/dev.clogic.dialogue/EditorResources/Textures/Icons/ActionBlock.svg", "Action Block")]
    public class ActionNode : DialogueContextNode<ActionNodeData>, IConnectionValidator
    {
        public override bool SupportExecution => false;
        public override bool SupportStartAction => false;
        public override bool SupportEndAction => false;

        public override void OnEnable()
        {
            DefaultColor = Color.dodgerBlue;
            Tooltip = "Defines action blocks to be executed at runtime";
        }

        protected override void DefineDialoguePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<ActionNode>(IDialogueGraphNode.IN_EXECUTION).WithDisplayName(string.Empty).WithConnectorUI(PortConnectorUI.Arrowhead).WithCapacity(PortCapacity.Multi).Build();
        }

        public override ActionNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portMap)
        {
            ActionNodeData data = new();

            ProcessChildBlocks(data, graph, portMap);

            return data;
        }

        public override void OnValidate(GraphLogger graphLogger)
        {
            base.OnValidate(graphLogger);

            if (BlockCount == 0)
                graphLogger.LogWarning("No actions specified", this);
        }

        public bool? CanConnect(IPort output, IPort input) => output.Name is IDialogueGraphNode.OUT_NODE_START or IDialogueGraphNode.OUT_NODE_END;
    }
}
