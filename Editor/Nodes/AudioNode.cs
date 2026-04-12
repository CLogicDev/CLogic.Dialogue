using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;

namespace CLogic.Systems.DialogueSystem.Editor
{
    [Serializable, UseWithContext(typeof(ActionNode)), Node("Events")]
    public class AudioNode : DialogueBlockNode<AudioNodeData>
    {
        public const string IN_AUDIOCLIP = "AudioClip";
        
        public override AudioNodeData ProcessNodeAsset(DialogueGraph graph, Dictionary<INode, int> nodeMap) => new()
        {
            audioClip = GetPortValue<AudioClip>(GetInputPortByName(IN_AUDIOCLIP))
        };
        
        protected override void OnDefinePorts(IPortDefinitionContext context) => context.AddInputPort<AudioClip>(IN_AUDIOCLIP).Build();
    }
}
