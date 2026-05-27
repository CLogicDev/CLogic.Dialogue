using System;
using UnityEngine;

namespace CLogic.Dialogue
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
        
        protected override void ProcessNode(AudioNodeData nodeDate, DialogueDirector director)
        {
            audioSource.clip = nodeDate.audioClip;
            audioSource.Play();
        }
    }
}
