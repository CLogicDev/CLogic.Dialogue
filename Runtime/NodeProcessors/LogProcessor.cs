using System;
using UnityEngine;

namespace CLogic.Dialogue
{
    [Serializable]
    public class LogNodeData : BlockNodeData
    {
        public string message;
    }

    [SingletonProcessor, AddComponentMenu("")]
    public class LogProcessor : DialogueProcessor<LogNodeData>
    {
        protected override void ProcessNode(LogNodeData nodeData, DialogueDirector director) => Debug.Log(nodeData.message);
    }
}
