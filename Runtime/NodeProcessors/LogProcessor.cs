using System;
using UnityEngine;
using CLogic.Dialogue.Provisioner;

namespace CLogic.Dialogue
{
    [Serializable]
    public class LogNodeData : BlockNodeData
    {
        public const string IN_MESSAGE = "Message";

        [Provision(IN_MESSAGE)]
        public string message;
    }

    [SingletonProcessor, AddComponentMenu("")]
    public class LogProcessor : DialogueProcessor<LogNodeData>
    {
        protected override void ProcessNode(LogNodeData nodeData, DialogueDirector director) => Debug.Log(nodeData.message);
    }
}
