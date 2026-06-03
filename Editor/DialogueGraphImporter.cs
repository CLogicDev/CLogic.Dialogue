using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CLogic.Dialogue;
using UnityEditor;

namespace CLogic.Dialogue.Editor
{
    [ScriptedImporter(1, DialogueEditorGraph.ASSET_EXTENSION)]
    public class DialogueGraphImporter : ScriptedImporter
    {
        private Dictionary<INode, int> nodeMap;
        
        public override void OnImportAsset(AssetImportContext context)
        {
            var editorGraph = GraphDatabase.LoadGraphForImporter<DialogueEditorGraph>(context.assetPath);
            var graphData = ScriptableObject.CreateInstance<DialogueGraph>();
            
            CreateNodeMap(editorGraph);
            ProcessNodes(editorGraph, graphData, context);
            
            if (graphData.startNodeID == -1)
                SetVariableStartNode();
            
            context.AddObjectToAsset("main", graphData);
            context.SetMainObject(graphData);
            
            // Make the input variable behave as the starter node (in case of sub graphing)
            void SetVariableStartNode()
            {
                foreach (IVariable variable in editorGraph.GetVariables())
                {
                    if (variable.VariableKind != VariableKind.Input)
                        continue;
                    
                    List<IVariableNode> nodeBuffer = new(1);
                    variable.GetNodes(nodeBuffer);
                    
                    INode starterNode = nodeBuffer[0].GetOutputPort(0).FirstConnectedPort?.GetNode();
                    
                    if (starterNode != null)
                        graphData.startNodeID = nodeMap[starterNode];
                    
                    return;
                }
            }
        }
        
        private void CreateNodeMap(DialogueEditorGraph editorGraph)
        {
            nodeMap = new Dictionary<INode, int>();
            List<IVariableNode> variableNodeBuffer = new(1); // Created outside to reduce heap allocations
            int id = 0;
            
            // Prevents asset subgraphs from ending abruptly
            MapSubGraphEndPoint();
            
            MapNodesRecursively(editorGraph.GetNodes());
            
            return;
            
            void MapSubGraphEndPoint()
            {
                if (editorGraph.IsSubGraphInstance)
                    return;
                
                IVariable outputNode = GetVariableForKind(editorGraph, VariableKind.Output);
                
                if (outputNode == null) // Only asset subgraphs should end with an output node
                    return;
                
                variableNodeBuffer.Clear();
                outputNode.GetNodes(variableNodeBuffer);
                
                if (variableNodeBuffer.Count <= 0)
                    return;
                
                // Output node should
                // be considered as a graceful exit
                nodeMap.Add(variableNodeBuffer[0], -2);
            }
            
            void MapNodesRecursively(IEnumerable<INode> nodes)
            {
                foreach (INode node in nodes)
                {
                    if (nodeMap.ContainsKey(node)) // Can occur in case of sub graph force mappings
                        continue;
                    
                    if (node is EndNode)
                        nodeMap.Add(node, -2);
                    
                    if (node is IDialogueGraphNode and not StartNode) // Start node is a special node which doesn't need an id
                        nodeMap.Add(node, id++);
                    
                    if (node is not ISubgraphNode subgraphNode)
                        continue;
                    
                    if (IsAssetSubGraphNode(subgraphNode))
                        HandleAssetSubGraph(subgraphNode);
                    else
                        ExpandSubGraph(subgraphNode);
                }
            }
            
            
            void HandleAssetSubGraph(ISubgraphNode subgraphNode)
            {
                subgraphNode.GetNodeOptionByName(DialogueEditorGraph.OP_EXPAND_SUBGRAPH).TryGetValue(out bool expandIntoParent);
                
                if (expandIntoParent)
                {
                    ExpandSubGraph(subgraphNode);
                    return;
                }
                
                nodeMap.Add(subgraphNode, id++);
            }
            
            void ExpandSubGraph(ISubgraphNode subgraphNode)
            {
                Graph subgraph = subgraphNode.GetSubgraph();
                MapNodesRecursively(subgraph.GetNodes());
                
                //Support input node
                variableNodeBuffer.Clear();
                IVariable subgraphInputVariable = GetVariableForKind(subgraph, VariableKind.Input);
                subgraphInputVariable.GetNodes(variableNodeBuffer); // There should always only be one here but no checks can be made yet due to API limits
                
                INode subgraphStarterNode = variableNodeBuffer[0].GetOutputPort(0).FirstConnectedPort.GetNode();
                
                nodeMap.Add(subgraphNode, nodeMap[subgraphStarterNode]);
                
                //Support output node
                variableNodeBuffer.Clear();
                IVariable subgraphOutputVariable = GetVariableForKind(subgraph, VariableKind.Output);
                subgraphOutputVariable.GetNodes(variableNodeBuffer); // There should always only be one here but no checks can be made yet due to API limits
                
                // Node the subgraph node (not the subgraph) points to inside the parent graph
                INode subgraphEndNode = subgraphNode.GetOutputPort(0).FirstConnectedPort.GetNode();
                
                int targetId;
                if (subgraphEndNode is EndNode) // If the subgraph points to an end node in the origin graph, set the variable node inside the subgraph to point to a graceful end
                {
                    targetId = -2; // Graceful end
                }
                else if (nodeMap.TryGetValue(subgraphEndNode, out int endNodeId))
                {
                    targetId = endNodeId;
                }
                else
                {
                    targetId = id;
                    
                    //Force map the output node to map to the subgraph node leading to it
                    nodeMap.TryAdd(subgraphEndNode, id++);
                }
                
                //Variable acts as node. Direct that node (from the subgraph) to the output node (of the parent graph)
                nodeMap.Add(variableNodeBuffer[0], targetId);
            }
            
            IVariable GetVariableForKind(Graph graph, VariableKind kind)
            {
                foreach (IVariable variable in graph.GetVariables())
                {
                    if (variable.VariableKind == kind)
                        return variable;
                }
                return null;
            }
        }
        
        private void ProcessNodes(DialogueEditorGraph editorGraph, DialogueGraph graph, AssetImportContext context)
        {
            var list = new List<(int id, DialogueNodeData data)>();
            
            ProcessNodesRecursively(editorGraph.GetNodes());
            
            graph.nodes = list
                    .OrderBy(x => x.id)
                    .Select(x => x.data)
                    .ToArray();
            
            return;
            
            void ProcessNodesRecursively(IEnumerable<INode> nodes)
            {
                HashSet<ScriptableObject> savedProvisions = new();
                int provisionedCount = 0;
                foreach (INode node in nodes)
                {
                    if (node is IScriptableObjectProvisionerNode provisionerNode)
                    {
                        ScriptableObject so = provisionerNode.GetScriptableObject();
                        if(savedProvisions.Add(so))
                            context.AddObjectToAsset("provision " + provisionedCount++, provisionerNode.GetScriptableObject());
                    }
                    
                    if (node is ISubgraphNode subgraphNode)
                    {
                        if (IsAssetSubGraphNode(subgraphNode))
                        {
                            DialogueNodeData subgraphData = ProcessAssetSubgraphNode(subgraphNode);
                            
                            if (subgraphData != null)
                                list.Add((nodeMap[subgraphNode], ProcessAssetSubgraphNode(subgraphNode)));
                        }
                        else
                            ProcessNodesRecursively(subgraphNode.GetSubgraph().GetNodes());
                        continue;
                    }
                    if (node is not IDialogueGraphNode dialogueNode)
                        continue;
                    
                    DialogueNodeData data = dialogueNode.ProcessNode(graph, nodeMap);
                    
                    if (data != null)
                        list.Add((nodeMap[node], data));
                }
            }
            
            DialogueNodeData ProcessAssetSubgraphNode(ISubgraphNode subgraphNode)
            {
                subgraphNode.GetNodeOptionByName(DialogueEditorGraph.OP_EXPAND_SUBGRAPH).TryGetValue(out bool expandIntoParent);
                
                if (expandIntoParent)
                {
                    ProcessNodesRecursively(subgraphNode.GetSubgraph().GetNodes());
                    return null;
                }
                
                SubGraphNodeData nodeData = new();
                
                GUID subgraphGuid = subgraphNode.GetSubgraph().AssetGuid;
                var subgraph = AssetDatabase.LoadAssetByGUID<DialogueGraph>(subgraphGuid);
                
                nodeData.graph = subgraph;
                
                nodeData.nextNodeID = nodeMap[subgraphNode.GetOutputPort(0).FirstConnectedPort.GetNode()];
                
                return nodeData;
            }
        }
        
        private bool IsAssetSubGraphNode(ISubgraphNode subgraphNode)
        {
            Type typeToQuery = subgraphNode.GetType().BaseType;
            PropertyInfo prop = typeToQuery.GetProperty("IsReferencingLocalSubgraph", BindingFlags.Instance | BindingFlags.Public);
            return !(bool)prop.GetValue(subgraphNode);
        }
    }
}
