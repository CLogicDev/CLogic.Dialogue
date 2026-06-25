using System;
using System.Collections.Generic;
using CLogic.Conditionals;
using CLogic.Dialogue.Editor;
using Unity.GraphToolkit.Editor;
using UnityEngine;
namespace CLogic.Dialogue.Conditionals.Editor
{
    [Serializable, UseWithGraph(typeof(DialogueEditorGraph)), Node("Provisioners", "", "Boolean Logic")]
    public class LogicalNode : ProvisionerNode<ConditionalEvaluator>, IScriptableObjectProvisionerNode
    {
        private const string OP_INPUT_COUNT = "InputCount";
        private const string OP_LOGICAL_TYPE = "LogicalType";
        
        private const string IN_EVALUATOR = "Evaluators";
        
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            base.OnDefineOptions(context);
            
            context.AddOption<int>(OP_INPUT_COUNT).WithDefaultValue(1).WithDisplayName("Inputs").Build();
            context.AddOption<LogicalEvaluator.LogicalOperationType>(OP_LOGICAL_TYPE).WithDisplayName("Logical Type").Build();
        }
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            GetNodeOptionByName(OP_INPUT_COUNT).TryGetValue(out int inputCount);
            
            for (int i = 0; i < inputCount; i++)
            {
                context.AddInputPort<ConditionalEvaluator>(IN_EVALUATOR + i).WithDisplayName("").Build();
            }
        }
        
        protected override ConditionalEvaluator GetProvisionedDataInternal()
        {
            GetNodeOptionByName(OP_LOGICAL_TYPE).TryGetValue(out LogicalEvaluator.LogicalOperationType type);
            
            var evaluator = ScriptableObject.CreateInstance<LogicalEvaluator>();
            
            evaluator.name = nameof(LogicalEvaluator);
            evaluator.hideFlags = HideFlags.HideInHierarchy;
            evaluator.evaluators = GetEvaluators().ToArray();
            evaluator.logicalOperationType = type;
            
            return evaluator;
        }
        
        public List<ConditionalEvaluator> GetEvaluators()
        {
            List<ConditionalEvaluator> evaluators = new();
            GetNodeOptionByName(OP_INPUT_COUNT).TryGetValue(out int inputCount);
            for (int i = 0; i < inputCount; i++)
            {
                var evaluator = IDialogueGraphNode.GetPortValue<ConditionalEvaluator>(GetInputPortByName(IN_EVALUATOR + i));
                
                if(evaluator == null)
                    continue;
                
                evaluators.Add(evaluator);
            }
            
            return evaluators;
        }
        
        public override void OnValidate(GraphLogger graphLogger)
        {
            //GetInputPortByName(IN_EVALUATOR).TryGetValue(out ConditionalEvaluator[] evaluators);
            GetNodeOptionByName(OP_LOGICAL_TYPE).TryGetValue(out LogicalEvaluator.LogicalOperationType logicalType);
            GetNodeOptionByName(OP_INPUT_COUNT).TryGetValue(out int inputCount);
            
            if (logicalType == LogicalEvaluator.LogicalOperationType.NOT && inputCount> 1)
            {
                graphLogger.LogWarning("NOT operation can only be applied to one operand. It will be applied only to the first element", this);
            }
            
        }
        public ScriptableObject GetScriptableObject() => GetProvisionedData<ConditionalEvaluator>();
    }
}
