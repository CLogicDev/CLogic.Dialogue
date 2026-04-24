using Unity.GraphToolkit.Editor;
using System.Collections.Generic;

namespace CLogic.Systems.DialogueSystem.Editor
{
    /// <summary>
    /// Use when the node is not directly part of the flow of dialogue, i.e. values that need to be evaluated at runtime <br></br>
    /// Otherwise look into <see cref="DialogueNode{T}"/>
    /// </summary>
    public interface IDialogueGraphNode
    {
        public DialogueNodeData ProcessNode(DialogueGraph graph, Dictionary<INode, int> nodeMap);
        
        public void OnValidate(GraphLogger graphLogger);
        
        public static TValue GetPortValue<TValue>(IPort port)
        {
            if (port == null)
                return default;
            
            if (port.IsConnected)
            {
                INode node = port.FirstConnectedPort.GetNode();
                switch (node)
                {
                    case IVariableNode variableNode:
                    {
                        variableNode.Variable.TryGetDefaultValue(out TValue value);
                        return value;
                    }
                    case IConstantNode constantNode:
                    {
                        constantNode.TryGetValue(out TValue value);
                        return value;
                    }
                }
                return default;
            }
            
            port.TryGetValue(out TValue fallback);
            return fallback;
        }
    }
}
