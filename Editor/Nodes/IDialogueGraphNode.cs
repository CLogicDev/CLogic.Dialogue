using Unity.GraphToolkit.Editor;
using System.Collections.Generic;

namespace CLogic.Systems.DialogueSystem.Editor
{
    internal interface IDialogueGraphNode
    {
        internal DialogueNodeData ProcessNode(DialogueGraph graph, Dictionary<INode, int> nodeMap);

        public void OnValidate(GraphLogger graphLogger);

        public static TValue GetPortValue<TValue>(IPort port)
        {
            if (port == null)
                return default;

            if (port.IsConnected && port.FirstConnectedPort.GetNode() is IVariableNode variableNode)
            {
                variableNode.Variable.TryGetDefaultValue(out TValue value);
                return value;
            }

            port.TryGetValue(out TValue fallback);
            return fallback;
        }
    }
}
