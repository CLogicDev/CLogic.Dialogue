using System;
using UnityEditor;
using System.Linq;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace CLogic.Dialogue.Editor
{
    /// <summary>
    /// Checks the connection between two ports. Output and Input is checked for the implementation and connection is only allowed if both nodes allow it
    /// </summary>
    /// <returns>true if the connection is valid, false otherwise. Null to fall back to default validation</returns>
    public interface IConnectionValidator
    {
        public bool? CanConnect(IPort output, IPort input);
    }
    
    [Serializable, Graph(ASSET_EXTENSION, GraphOptions.SupportsSubgraphs)]
    public partial class DialogueEditorGraph : Graph
    {
        public const string ASSET_EXTENSION = "cdg";
        
        [MenuItem("Assets/Create/CLogic/Dialogue Graph/New Graph", priority = 1)]
        private static void CreateAssetFile() => GraphDatabase.PromptInProjectBrowserToCreateNewAsset<DialogueEditorGraph>();
        
        public override void OnGraphChanged(GraphLogger graphLogger)
        {
            IEnumerable<INode> nodes = GetNodes();
            
            int connectedStartNodes = 0;
            foreach (INode node in nodes)
            {
                if (node is IDialogueGraphNode dialogueNode)
                    dialogueNode.OnValidate(graphLogger);
                
                if(node is IProvisionerNode provisionerNode)
                    provisionerNode.OnValidate(graphLogger);
                
                if (node is StartNode && node.GetOutputPortByName(StartNode.OUT_START).IsConnected)
                    connectedStartNodes++;
            }
            
            if (connectedStartNodes > 1)
                graphLogger.LogError("Multiple connected start nodes detected. Only one connected start node should exist", this);
            
            if (IsSubGraphInstance)
                ValidateSubGraph(graphLogger);
            
            
        }
        
        public override bool IsConnectionAllowed(IPort output, IPort input)
        {
            INode inputNode = input.GetNode();
            INode outputNode = output.GetNode();
            
            bool? canInputConnect = ValidateForNode(inputNode);
            bool? canOutputConnect = ValidateForNode(outputNode);
            
            if(!canInputConnect.HasValue && !canOutputConnect.HasValue)
                return base.IsConnectionAllowed(output, input);
            
            return (canInputConnect ?? true) && (canOutputConnect ?? true);
            
            bool? ValidateForNode(INode node)
            {
                if (node is not IConnectionValidator validator)
                    return null;
                
                return validator.CanConnect(output, input);
            }
        }
    }
}
