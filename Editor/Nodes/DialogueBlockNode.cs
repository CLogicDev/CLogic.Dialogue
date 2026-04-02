using System;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;

namespace CLogic.Systems.DialogueSystem.Editor
{
    [Serializable]
    public abstract class DialogueBlockNode<T> : BlockNode, IDialogueGraphNode where T : DialogueNodeData
    {
        public virtual void OnValidate(GraphLogger graphLogger) { }

        public abstract T ProcessNodeAsset(DialogueGraph graph, Dictionary<INode, int> nodeMap);

        protected TValue GetPortValue<TValue>(IPort port) => IDialogueGraphNode.GetPortValue<TValue>(port);

        DialogueNodeData IDialogueGraphNode.ProcessNode(DialogueGraph graph, Dictionary<INode, int> nodeMap) => ProcessNodeAsset(graph, nodeMap);
    }
}
