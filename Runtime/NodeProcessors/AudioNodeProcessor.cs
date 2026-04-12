using System;
using UnityEngine;

namespace CLogic.Systems.DialogueSystem
{
    [Serializable]
    public class AudioNodeData : BlockNodeData
    {
        public AudioClip audioClip;
    }
    
    public class AudioNodeProcessor : DialogueNodeProcessor<AudioNodeData>
    {
        [SerializeField]
        private AudioSource audioSource;
        
        public override void ProcessNode(AudioNodeData dialogueNode, DialogueDirector director)
        {
            audioSource.clip = dialogueNode.audioClip;
            audioSource.Play();
        }
    }
}
