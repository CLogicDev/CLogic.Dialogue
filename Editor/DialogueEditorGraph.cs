using System;
using UnityEditor;
using System.Linq;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;
using UnityEngine;

namespace CLogic.Dialogue.Editor
{
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
        #if UNITY_6000_6_OR_NEWER
        public override bool IsConnectionAllowed(IPort output, IPort input)
        {
            if (output.Name == DialogueNode<DialogueNodeData>.OUT_NODE_START)
                return input.GetNode() is ActionNode;
            
            if (input.GetNode() is ActionNode)
                return output.Name is DialogueNode<DialogueNodeData>.OUT_NODE_START or DialogueNode<DialogueNodeData>.OUT_NODE_END;
            
            return base.IsConnectionAllowed(output, input);
        }
        #endif
    }
}
