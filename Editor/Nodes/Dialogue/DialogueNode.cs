using System;
using System.Linq;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using System.Reflection;
using CLogic.Dialogue;
using UnityEngine;

namespace CLogic.Dialogue.Editor
{
    /// <summary>
    /// Use when the node is part of the dialogue flow<br></br>
    /// Otherwise look into <see cref="IDialogueGraphNode"/>
    /// </summary>
    [Serializable]
    public abstract class DialogueNode<T> : Node, IDialogueGraphNode where T : DialogueNodeData
    {
        public const string IN_EXECUTION = "In";
        public const string OUT_EXECUTION = "Out";
        
        public const string OUT_NODE_END = "End";
        public const string OUT_NODE_START = "Start";
        
        public const string OP_NODE_EVENTS = "UseEvents";
        
        public virtual bool SupportsStartAction => true;
        public virtual bool SupportsEndAction => true;
        
        [field: SerializeField]
        public bool IsFirstCreation { get; protected set; } = true;
        
        public virtual void OnValidate(GraphLogger graphLogger)
        {
            IPort outputPort = GetOutputPortByName(OUT_EXECUTION);
            
            if (outputPort != null)
            {
                List<IPort> connectedPorts = new();
                
                outputPort.GetConnectedPorts(connectedPorts);
                
                foreach (IPort port in connectedPorts)
                {
                    INode node = port?.GetNode();
                    
                    if (node is ActionNode)
                        graphLogger.LogError("Action node cannot be used as execution output", this);
                }
                
                switch (connectedPorts.Count)
                {
                    case 0:
                        graphLogger.Log("Node output not connected, the graph will end by default", this);
                    break;
                    
                    case > 1:
                        graphLogger.LogError("Multiple execution output links are not allowed", this);
                    break;
                }
                
            }
            
            if(!SupportsStartAction || !SupportsEndAction)
                return;
            
            if (!GetNodeOptionByName(OP_NODE_EVENTS).TryGetValue(out bool shouldUseEvents) || !shouldUseEvents)
                return;
            
            if (SupportsStartAction)
            {
                IPort connectedPort = GetOutputPortByName(OUT_NODE_START)?.FirstConnectedPort;
                
                INode connectedNode = connectedPort.GetNode();
                if (connectedNode is not null and not ActionNode)
                    graphLogger.LogError("Start node must be connected to an action node", this);
            }
            
            if (SupportsEndAction)
            {
                IPort connectedPort = GetOutputPortByName(OUT_NODE_END)?.FirstConnectedPort;
                
                INode connectedNode = connectedPort.GetNode();
                if (connectedNode is not null and not ActionNode)
                    graphLogger.LogError("End node must be connected to an action node", this);
            }
        }
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            if (SupportsStartAction || SupportsEndAction)
                context.AddOption<bool>(OP_NODE_EVENTS).WithDisplayName("Use Events").Build();
        }
        
        protected sealed override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            DefineDialoguePorts(context);
            InitFinished();
        }
        protected abstract void DefineDialoguePorts(IPortDefinitionContext context);
        
        public abstract T ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portMap);
        
        public virtual void CreateNodeLink(T node, Dictionary<IPort, int> portMap)
        {
            // Execution link
            IPort executionPort = GetOutputPortByName(OUT_EXECUTION)?.FirstConnectedPort;
            
            if (executionPort == null)
                return;
            
            INode connectedNode = executionPort.GetNode();
            node.nextNodeID = portMap.GetValueOrDefault(executionPort, -1);
            
            // Action link
            if (SupportsStartAction)
            {
                IPort actionPort = GetOutputPortByName( OUT_NODE_START)?.FirstConnectedPort;
                
                if (actionPort != null)
                    node.startNodeActionID = portMap.GetValueOrDefault(actionPort, -1);
            }
            
            if (SupportsEndAction)
            {
                IPort connectedPort = GetOutputPortByName(OUT_NODE_END)?.FirstConnectedPort;
                
                if (connectedPort != null)
                    node.endNodeActionID = portMap.GetValueOrDefault(connectedPort, -1);
            }
        }
        
        protected void CreateDefaultExecutionPorts(IPortDefinitionContext context)
        {
            var inputPort = context.AddInputPort<IDialogueGraphNode>(IN_EXECUTION).WithConnectorUI(PortConnectorUI.Arrowhead).WithDisplayName(string.Empty).Build();
            context.AddOutputPort<IDialogueGraphNode>(OUT_EXECUTION).WithConnectorUI(PortConnectorUI.Arrowhead).WithDisplayName(string.Empty).Build();
            
            PropertyInfo propertyInfo = inputPort.GetType().GetProperty("Capacity", BindingFlags.Instance | BindingFlags.Public);
            Type portCapacityType = propertyInfo.PropertyType;
            object multiCapacity = Enum.Parse(portCapacityType, "Multi");
            propertyInfo.SetValue(inputPort, multiCapacity);
            
            if(!SupportsStartAction && !SupportsEndAction)
                return;
            
            if (GetNodeOptionByName(OP_NODE_EVENTS).TryGetValue(out bool shouldUseEvents) && shouldUseEvents)
            {
                if (SupportsStartAction)
                    context.AddOutputPort<ActionNode>(OUT_NODE_START).WithDisplayName("Start").Build();
                
                if (SupportsEndAction)
                    context.AddOutputPort<ActionNode>(OUT_NODE_END).WithDisplayName("End").Build();
            }
        }
        
        protected TValue GetPortValue<TValue>(IPort port) => IDialogueGraphNode.GetPortValue<TValue>(port);
        
        DialogueNodeData IDialogueGraphNode.ProcessNode(DialogueGraph graph, Dictionary<IPort, int> portMap) => ProcessNodeAsset(graph, portMap);
        
        private void InitFinished()
        {
            if (IsFirstCreation)
            {
                OnFirstCreation();
                IsFirstCreation = false;
            }
            
            PostInit();
        }
        
        protected virtual void PostInit() 
        {}
        
        // NOTE: Will not be called on a duplicated node
        protected virtual void OnFirstCreation()
        {}
    }
}
