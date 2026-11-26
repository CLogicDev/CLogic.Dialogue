using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using CLogic.Dialogue.Provisioner;

namespace CLogic.Dialogue.Editor
{
    [Serializable, UseWithGraph(typeof(DialogueEditorGraph)), Node("Basic", "AudioRandomContainer On Icon", "Random Text")]
    public class RandomTextNode : RuntimeProvisionerNode<string, RandomTextData>
    {
        public const string IN_OPTIONS = "Options";

        public override void OnEnable()
        {
            DefaultColor = Color.coral;
            Tooltip = "Provides random text from a list of options";
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<string[]>(IN_OPTIONS).Build();
            context.AddOutputPort<string>(IProvisionerNode.OUT_PROVISION).WithDisplayName("Text").Build();
        }

        public override void ProcessNodeCore(ref RandomTextData provisionData, DialogueGraph graph, Dictionary<IPort, int> portMap)
        {
            provisionData.options = IDialogueGraphNode.GetPortValue<string[]>(GetInputPortByName(IN_OPTIONS));
        }
    }
}
