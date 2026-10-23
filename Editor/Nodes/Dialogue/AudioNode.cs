using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;

namespace CLogic.Dialogue.Editor
{
    [Serializable, UseWithContext(typeof(ActionNode)), Node("Events", "", "Audio Player")]
    public class AudioNode : DialogueBlockNode<AudioNodeData>
    {
        public override bool SupportStartAction => false;
        public override bool SupportEndAction => false;

        public const string IN_AUDIOCLIP = "AudioClip";

        public override void OnEnable() => Tooltip = "Plays the specified audio clip";

        public override AudioNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portmap) => new()
        {
            audioClip = GetPortValue<AudioClip>(GetInputPortByName(IN_AUDIOCLIP))
        };

        protected override void OnDefinePorts(IPortDefinitionContext context) => context.AddInputPort<AudioClip>(IN_AUDIOCLIP).Build();
    }
}
