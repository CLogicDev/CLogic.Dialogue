using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using CLogic.Dialogue;

namespace CLogic.Dialogue.Editor
{
    /// <summary>
    /// Use when the node is not directly part of the flow of dialogue, i.e. values that need to be evaluated at runtime <br></br>
    /// Otherwise look into <see cref="DialogueNode{T}"/>
    /// </summary>
    public interface IDialogueGraphNode
    {
        public DialogueNodeData ProcessNode(DialogueGraph graph, Dictionary<INode, int> nodeMap);
        
        public void OnValidate(GraphLogger graphLogger);
        
        public static bool TryGetPortValue<TValue>(IPort port, out TValue value)
        {
            if(port == null)
            {
                value = default;
                return false;
            }

            if (port.IsConnected)
            {
                INode node = port.FirstConnectedPort.GetNode();
                switch (node)
                {
                    case IVariableNode variableNode:
                    {
                        variableNode.Variable.TryGetDefaultValue(out value);
                        return true;
                    }
                    case IConstantNode constantNode:
                    {
                        constantNode.TryGetValue(out value);
                        return true;
                    }
                    
                    case IProvisionerNode provisionerNode when typeof(TValue).IsAssignableFrom(provisionerNode.HandledType):
                    {
                       value = provisionerNode.GetProvisionedData<TValue>();
                       return true;
                    }
                }
                value = default;
                return false;
            }
            
            bool hasInlinedValue = port.TryGetValue(out value) && value != null;
            
            return hasInlinedValue;
        }

        public static TValue GetPortValue<TValue>(IPort port)
        {
            TryGetPortValue<TValue>(port, out var value);
            return value;
        }
    }
}
