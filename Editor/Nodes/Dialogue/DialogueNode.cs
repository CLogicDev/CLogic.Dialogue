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
    public abstract class DialogueNode<T> : Node, IDialogueGraphNode, IConnectionValidator where T : DialogueNodeData
    {
        
        public virtual bool SupportsStartAction => true;
        public virtual bool SupportsEndAction => true;
        
        [field: SerializeField]
        public bool IsFirstCreation { get; protected set; } = true;
        
        public virtual void OnValidate(GraphLogger graphLogger)
        {
            IDialogueGraphNode.ValidateExecution(graphLogger, this);
            IDialogueGraphNode.ValidateActionLinks(graphLogger, this, SupportsStartAction, SupportsEndAction);
        }
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            if (SupportsStartAction || SupportsEndAction)
                context.AddOption<bool>(IDialogueGraphNode.OP_NODE_EVENTS).WithDisplayName("Use Events").Build();
        }
        
        protected sealed override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            DefineDialoguePorts(context);
            InitFinished();
        }
        protected abstract void DefineDialoguePorts(IPortDefinitionContext context);
        
        public abstract T ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portMap);
        
        public void CreateNodeLink(T node, Dictionary<IPort, int> portMap)
        {
            IDialogueGraphNode.CreateExecutionNodeLink(node, portMap, this);
            IDialogueGraphNode.CreateActionNodeLink(node, portMap, this, SupportsStartAction, SupportsEndAction);
        }
        
        protected void CreateDefaultExecutionPorts(IPortDefinitionContext context)
        {
            context.AddInputPort<IDialogueGraphNode>(IDialogueGraphNode.IN_EXECUTION).WithConnectorUI(PortConnectorUI.Arrowhead).WithDisplayName(string.Empty).WithCapacity(PortCapacity.Multi).Build();
            context.AddOutputPort<IDialogueGraphNode>(IDialogueGraphNode.OUT_EXECUTION).WithConnectorUI(PortConnectorUI.Arrowhead).WithDisplayName(string.Empty).WithCapacity(PortCapacity.Single).Build();
            
            if(!SupportsStartAction && !SupportsEndAction)
                return;
            
            if (GetNodeOptionByName(IDialogueGraphNode.OP_NODE_EVENTS).TryGetValue(out bool shouldUseEvents) && shouldUseEvents)
            {
                if (SupportsStartAction)
                    context.AddOutputPort<ActionNode>(IDialogueGraphNode.OUT_NODE_START).WithDisplayName("Start").Build();
                
                if (SupportsEndAction)
                    context.AddOutputPort<ActionNode>(IDialogueGraphNode.OUT_NODE_END).WithDisplayName("End").Build();
            }
        }
        
        protected TValue GetPortValue<TValue>(IPort port) => IDialogueGraphNode.GetPortValue<TValue>(port);
        
        public DialogueNodeData ProcessNode(DialogueGraph graph, Dictionary<IPort, int> portMap) => ProcessNodeAsset(graph, portMap);
        
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
        
        public virtual bool? CanConnect(IPort output, IPort input)
        {
            if (input.GetNode() is not IDialogueGraphNode)
                return null;
            
            if (output.GetNode() is not IDialogueGraphNode)
                return null;
            
            if(output.Name is IDialogueGraphNode.OUT_NODE_START or IDialogueGraphNode.OUT_NODE_END)
                return input.GetNode() is ActionNode;
            
            if (output.Name != IDialogueGraphNode.OUT_EXECUTION)
                return null;
            
            return input.Name == IDialogueGraphNode.IN_EXECUTION && output.Name == IDialogueGraphNode.OUT_EXECUTION;
        }
    }
}
