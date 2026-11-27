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
            context.AddInputPort<Dictionary<string, float>>(IN_OPTIONS).WithTooltip("List of text options with a probability weight").Build();
            context.AddOutputPort<string>(IProvisionerNode.OUT_PROVISION).WithDisplayName("Text").Build();
        }

        public override void ProcessNodeCore(ref RandomTextData provisionData, DialogueGraph graph, Dictionary<IPort, int> portMap)
        {
            provisionData.options = IDialogueGraphNode.GetPortValue<Dictionary<string, float>>(GetInputPortByName(IN_OPTIONS));
        }

        public override void OnValidate(GraphLogger graphLogger)
        {
            var options = IDialogueGraphNode.GetPortValue<Dictionary<string, float>>(GetInputPortByName(IN_OPTIONS));

            float totalProbability = 0f;
            foreach (var pair in options)
            {
                if (pair.Value < 0f)
                {
                    graphLogger.LogWarning($"Option '{pair.Key}' has a negative probability weight. This option will be ignored.", this);
                    continue;
                }

                totalProbability += pair.Value;
            }

            if (totalProbability <= 0f)
                graphLogger.LogWarning("Total probability weight of all options is zero. No text will be provided.", this);
        }
    }
}
