using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace CLogic.Dialogue.Editor
{
    /// <summary>
    /// Use when the node is not directly part of the flow of dialogue, i.e. values that need to be evaluated at runtime <br></br>
    /// Otherwise look into <see cref="DialogueNode{T}"/>
    /// </summary>
    public interface IDialogueGraphNode
    {
        public const string IN_EXECUTION = "In";
        public const string OUT_EXECUTION = "Out";
        
        public const string OUT_NODE_END = "End";
        public const string OUT_NODE_START = "Start";
        
        public const string OP_NODE_EVENTS = "UseEvents";
        
        public const int INVALID_END = -1;
        public const int GRACEFUL_END = -2;
        
        public DialogueNodeData ProcessNode(DialogueGraph graph, Dictionary<IPort, int> portMap);
        
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
            TryGetPortValue(port, out TValue value);
            return value;
        }
        
        #region Validations
        
        public static void ValidateExecution(GraphLogger graphLogger, INode origin)
        {
            IPort outputPort = origin.GetOutputPortByName(OUT_EXECUTION);
            
            if (outputPort == null)
                return;
            
            List<IPort> connectedPorts = new();
            
            outputPort.GetConnectedPorts(connectedPorts);
            
            switch (connectedPorts.Count)
            {
                case 0:
                    graphLogger.Log("Node output not connected, the graph will end by default", origin, new GraphLogAction("Add End Node", obj =>
                    {
                        origin.Graph.UndoBeginRecordGraph("Add End Node");
                        var endNode = new EndNode();
                        endNode.Position = origin.Position;
                        endNode.Position += Vector2.right * 300;
                        endNode.Position += Vector2.up * 32f;
                        origin.Graph.AddNode(endNode);
                        origin.Graph.Connect(outputPort, endNode.GetInputPort(0));
                        origin.Graph.UndoEndRecordGraph();
                    }));
                break;
                
                case > 1:
                    graphLogger.LogError("Multiple execution output links are not allowed", origin);
                break;
            }
        }
        
        public static void ValidateActionLinks(GraphLogger graphLogger, INode origin, bool supportsStartAction, bool supportsEndAction)
        {
            if(!supportsStartAction && !supportsEndAction)
                return;
            
            if (!origin.GetNodeOptionByName(OP_NODE_EVENTS).TryGetValue(out bool shouldUseEvents) || !shouldUseEvents)
                return;
            
            if (supportsStartAction)
            {
                IPort connectedPort = origin.GetOutputPortByName(OUT_NODE_START)?.FirstConnectedPort;
                
                INode connectedNode = connectedPort.GetNode();
                if (connectedNode is not null and not ActionNode)
                    graphLogger.LogError("Start node must be connected to an action node", origin);
            }
            
            if (supportsEndAction)
            {
                IPort connectedPort = origin.GetOutputPortByName(OUT_NODE_END)?.FirstConnectedPort;
                
                INode connectedNode = connectedPort.GetNode();
                if (connectedNode is not null and not ActionNode)
                    graphLogger.LogError("End node must be connected to an action node", origin);
            }
        }
        
        #endregion
        
        #region Node Linkages
        
        public static void CreateExecutionNodeLink(DialogueNodeData nodeData, Dictionary<IPort, int> portMap, INode origin)
        {
            IPort executionPort = origin.GetOutputPortByName(OUT_EXECUTION)?.FirstConnectedPort;
            
            if (executionPort == null)
                return;
            
            nodeData.nextNodeID = portMap.GetValueOrDefault(executionPort, INVALID_END);
            nodeData.execInputPortHash = origin.GetInputPortByName(IN_EXECUTION)?.ID ?? new Hash128();
            nodeData.execOutputPortHash = origin.GetOutputPortByName(OUT_EXECUTION)?.ID ?? new Hash128();
        }
        
        public static void CreateActionNodeLink(DialogueNodeData nodeData, Dictionary<IPort, int> portMap, INode origin, bool supportsStartAction, bool supportsEndAction)
        {
            if (supportsStartAction)
            {
                IPort actionPort = origin.GetOutputPortByName(OUT_NODE_START)?.FirstConnectedPort;
                
                if (actionPort != null)
                    nodeData.startNodeActionID = portMap.GetValueOrDefault(actionPort, INVALID_END);
            }
            
            if (supportsEndAction)
            {
                IPort connectedPort = origin.GetOutputPortByName(OUT_NODE_END)?.FirstConnectedPort;
                
                if (connectedPort != null)
                    nodeData.endNodeActionID = portMap.GetValueOrDefault(connectedPort, INVALID_END);
            }
        }
        
        #endregion
    }
}
