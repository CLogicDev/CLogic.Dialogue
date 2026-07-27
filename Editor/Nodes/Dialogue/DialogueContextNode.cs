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
        public virtual bool SupportStartAction => true;
        public virtual bool SupportEndAction => true;
        
        public virtual bool SupportExecution => true;
        
        [field: SerializeField]
        public bool IsFirstCreation { get; protected set; } = true;
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            if (SupportStartAction || SupportEndAction)
                context.AddOption<bool>(IDialogueGraphNode.OP_NODE_EVENTS).WithDisplayName("Use Events").Build();
        }
        
        protected sealed override void OnDefinePorts(IPortDefinitionContext context)
        {
            if (SupportExecution)
            {
                context.AddInputPort<IDialogueGraphNode>(IDialogueGraphNode.IN_EXECUTION).WithDisplayName(string.Empty).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
                context.AddOutputPort<IDialogueGraphNode>(IDialogueGraphNode.OUT_EXECUTION).WithDisplayName(string.Empty).WithConnectorUI(PortConnectorUI.Arrowhead).Build();
            }

            if(SupportStartAction || SupportEndAction)
            {
                if(GetNodeOptionByName(IDialogueGraphNode.OP_NODE_EVENTS).TryGetValue(out bool shouldUseEvents) && shouldUseEvents)
                {
                    if(SupportStartAction)
                        context.AddOutputPort<ActionNode>(IDialogueGraphNode.OUT_NODE_START).WithDisplayName("Start").Build();

                    if(SupportEndAction)
                        context.AddOutputPort<ActionNode>(IDialogueGraphNode.OUT_NODE_END).WithDisplayName("End").Build();
                }
            }
            DefineDialoguePorts(context);
            InitFinished();
        }
        
        protected virtual void DefineDialoguePorts(IPortDefinitionContext context)
        {}
        
        public virtual void OnValidate(GraphLogger graphLogger)
        {
            if (SupportExecution)
                IDialogueGraphNode.ValidateExecution(graphLogger, this);
            
            foreach (BlockNode block in BlockNodes)
            {
                if (block is IDialogueGraphNode dialogueNode)
                    dialogueNode.OnValidate(graphLogger);
            }
            
            IDialogueGraphNode.ValidateActionLinks(graphLogger, this, SupportStartAction, SupportEndAction);
        }
        
        public abstract T ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portMap);
        
        protected void ProcessChildBlocks(T nodeData, DialogueGraph graph, Dictionary<IPort, int> portMap)
        {
            foreach (BlockNode block in BlockNodes)
            {
                if (block is not IDialogueGraphNode dialogueNode)
                    continue;
                
                DialogueNodeData blockNodeData = dialogueNode.ProcessNode(graph, portMap);
                nodeData.childBlocks.Add(blockNodeData);
            }
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
