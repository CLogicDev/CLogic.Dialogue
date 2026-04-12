using System.Collections.Generic;
using Unity.GraphToolkit.Editor;

namespace CLogic.Systems.DialogueSystem.Editor
{
    // Handles sub graph logic for the main graph
    public partial class DialogueEditorGraph
    {
        // Determines whether the data of the subgraph should be expanded into the main graph or processed separately
        public const string OP_EXPAND_SUBGRAPH = "ExpandedSubgraph";
        
        public bool IsSubGraphInstance { get; private set; } = false;
        
        protected override void OnDefineSubgraphNodeOptions(Node.IOptionDefinitionContext context)
        {
            context.AddOption<bool>(OP_EXPAND_SUBGRAPH).WithDisplayName("Expand Into Parent Graph").Build();
            
            IsSubGraphInstance = true;
        }
        
        private void ValidateSubGraph(GraphLogger graphLogger)
        {
            IVariable inputVariable = null;
            IVariable outputVariable = null;
            
            foreach (IVariable variable in GetVariables())
            {
                switch (variable.VariableKind)
                {
                    case VariableKind.Input when inputVariable == null:
                        inputVariable = variable;
                    break;
                    case VariableKind.Input:
                        graphLogger.LogError("Multiple input variables detected. Only one input variable should exist", this);
                        return;
                    
                    case VariableKind.Output when outputVariable == null:
                        outputVariable = variable;
                    break;
                    case VariableKind.Output:
                        graphLogger.LogError("Multiple output variables detected. Only one output variable should exist", this);
                        return;
                }
            }
            
            if (inputVariable == null)
            {
                graphLogger.LogError("No input found for subgraph", this);
                return;
            }
            
            if (outputVariable == null)
            {
                graphLogger.LogError("No output variable for subgraph", this);
                return;
            }
            
            List<IVariableNode> nodeBuffer = new(1);
            List<IPort> portBuffer = new(1);
            
            inputVariable.GetNodes(nodeBuffer);
            switch (nodeBuffer.Count)
            {
                case > 1:
                    graphLogger.LogError("Multiple input variables detected. Only one input should exist", inputVariable);
                    return;
                case 0:
                    graphLogger.LogError("No input connected. Please ensure the graph has at least one input", inputVariable);
                    return;
            }
            
            IVariableNode inputVariableNode = nodeBuffer[0];
            IPort inputVariablePort = inputVariableNode.GetOutputPort(0);
            
            inputVariablePort.GetConnectedPorts(portBuffer);
            
            switch (portBuffer.Count)
            {
                case 0:
                    graphLogger.LogError("No input connected", inputVariablePort);
                    return;
                case > 1:
                    graphLogger.LogError("Multiple inputs detected. Please ensure only one input path exists", inputVariableNode);
                    return;
            }
            
            outputVariable.GetNodes(nodeBuffer);
            switch (nodeBuffer.Count)
            {
                case > 1:
                    graphLogger.LogError("Multiple output variables detected. Only one output should exist", outputVariable);
                    return;
                case 0:
                    graphLogger.LogError("No output connected. Please ensure the graph has at least one output", outputVariable);
                    return;
            }
            
            IVariableNode outputVariableNode = nodeBuffer[0];
            IPort outputVariablePort = outputVariableNode.GetInputPort(0);
            
            outputVariablePort.GetConnectedPorts(portBuffer);
            switch (portBuffer.Count)
            {
                case 0:
                    graphLogger.LogError("No output connected", inputVariablePort);
                    return;
                case > 1:
                    graphLogger.LogError("Multiple outputs detected. Please ensure only one output path exists", inputVariableNode);
                    return;
            }
        }
    }
}
