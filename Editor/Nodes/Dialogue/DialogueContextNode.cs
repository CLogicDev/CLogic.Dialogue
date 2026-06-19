using System;
using System.Linq;
using System.Collections.Generic;
using CLogic.Dialogue;
using Unity.GraphToolkit.Editor;
using UnityEngine;

namespace CLogic.Dialogue.Editor
{
    [Serializable]
    public abstract class DialogueContextNode<T> : ContextNode, IDialogueGraphNode where T : ContextNodeData
    {
        public const string IN_EXECUTION = "In";
        public const string OUT_EXECUTION = "Out";
        
        public const string OUT_NODE_END = "End";
        public const string OUT_NODE_START = "Start";
        
        public const string OP_NODE_EVENTS = "UseEvents";
        
        public virtual bool SupportStartAction => true;
        public virtual bool SupportEndAction => true;
        
        public virtual bool SupportExecution => true;
        
        [field: SerializeField]
        public bool IsFirstCreation { get; protected set; } = true;
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            if (SupportStartAction || SupportEndAction)
                context.AddOption<bool>(OP_NODE_EVENTS).WithDisplayName("Use Events").Build();
        }
        
        protected sealed override void OnDefinePorts(IPortDefinitionContext context)
        {
            if (SupportExecution)
            {
                context.AddInputPort<IDialogueGraphNode>(IN_EXECUTION).WithDisplayName(string.Empty).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
                context.AddOutputPort<IDialogueGraphNode>(OUT_EXECUTION).WithDisplayName(string.Empty).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            }

            if(SupportStartAction || SupportEndAction)
            {
                if(GetNodeOptionByName(OP_NODE_EVENTS).TryGetValue(out bool shouldUseEvents) && shouldUseEvents)
                {
                    if(SupportStartAction)
                        context.AddOutputPort<ActionNode>(OUT_NODE_START).WithDisplayName("Start").Build();

                    if(SupportEndAction)
                        context.AddOutputPort<ActionNode>(OUT_NODE_END).WithDisplayName("End").Build();
                }
            }
            DefineDialoguePorts(context);
            InitFinished();
        }
        
        protected virtual void DefineDialoguePorts(IPortDefinitionContext context)
        {}
        
        public virtual void OnValidate(GraphLogger graphLogger)
        {
            List<IPort> connectedPorts = new();
            
            if (SupportExecution)
            {
                GetOutputPortByName(OUT_EXECUTION)?.GetConnectedPorts(connectedPorts);
                
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
            
            foreach (BlockNode block in BlockNodes)
            {
                if (block is IDialogueGraphNode dialogueNode)
                    dialogueNode.OnValidate(graphLogger);
            }
            
            if(!SupportStartAction || !SupportEndAction)
                return;
            
            if (GetNodeOptionByName(OP_NODE_EVENTS).TryGetValue(out bool shouldUseEvents) && shouldUseEvents)
            {
                if (SupportStartAction)
                {
                    IPort connectedPort = GetOutputPortByName(OUT_NODE_START)?.FirstConnectedPort;
                    
                    INode connectedNode = connectedPort.GetNode();
                    if (connectedNode is not null and not ActionNode)
                        graphLogger.LogError("Start node must be connected to an action node", this);
                }
                
                if (SupportEndAction)
                {
                    IPort connectedPort = GetOutputPortByName(OUT_NODE_END)?.FirstConnectedPort;
                    
                    INode connectedNode = connectedPort.GetNode();
                    if (connectedNode is not null and not ActionNode)
                        graphLogger.LogError("End node must be connected to an action node", this);
                }
            }
        }
        
        public abstract T ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portMap);
        
        protected void ProcessChildBlocks(T nodeData, DialogueGraph graph, Dictionary<IPort, int> portMap)
        {
            foreach (BlockNode block in BlockNodes)
            {
                if (block is IDialogueGraphNode dialogueNode)
                {
                    DialogueNodeData blockNodeData = dialogueNode.ProcessNode(graph, portMap);
                    nodeData.childBlocks.Add(blockNodeData);
                }
            }
        }
        
        private void CreateActionNodeLink(T node, Dictionary<IPort, int> portMap)
        {
            if (SupportStartAction)
            {
                IPort connectedPort = GetOutputPortByName( OUT_NODE_START)?.FirstConnectedPort;
                
                if (connectedPort != null)
                    node.startNodeActionID = portMap.GetValueOrDefault(connectedPort, -1);
            }
            
            if (SupportEndAction)
            {
                IPort connectedPort = GetOutputPortByName(OUT_NODE_END)?.FirstConnectedPort;
                
                if (connectedPort != null)
                    node.endNodeActionID = portMap.GetValueOrDefault(connectedPort, -1);
            }
        }
        
        //TODO: Find a way to remove code dupe from DialogueNode.cs
        public virtual void CreateExecutionNodeLink(T node, Dictionary<IPort, int> portMap)
        {
            IPort connectedPort = GetOutputPortByName(OUT_EXECUTION)?.FirstConnectedPort;
            
            if (connectedPort == null)
                return;
            
            INode connectedNode = connectedPort.GetNode();
            node.nextNodeID = portMap.GetValueOrDefault(connectedPort, -1);
        }
        
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
