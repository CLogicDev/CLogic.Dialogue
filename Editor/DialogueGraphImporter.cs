using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CLogic.Dialogue;
using CLogic.Dialogue.Provisioner;
using UnityEditor;

namespace CLogic.Dialogue.Editor
{
    [ScriptedImporter(1, DialogueEditorGraph.ASSET_EXTENSION)]
    public class DialogueGraphImporter : ScriptedImporter
    {
        private Dictionary<INode, int> nodeMap;
        private Dictionary<IPort, int> portMap;
        
        public override void OnImportAsset(AssetImportContext context)
        {
            var editorGraph = GraphDatabase.LoadGraphForImporter<DialogueEditorGraph>(context.assetPath);
            var graphData = ScriptableObject.CreateInstance<DialogueGraph>();
            
            CreateNodeMap(editorGraph, graphData);
            ProcessNodes(editorGraph, graphData, context);
            
            if (graphData.startNodeID == IDialogueGraphNode.INVALID_END)
                SetVariableStartNode();
            
            graphData.graphHash = editorGraph.ID;
            
            context.AddObjectToAsset("main", graphData);
            context.SetMainObject(graphData);
            
            // Make the input variable behave as the starter node (in case of sub graphing)
            void SetVariableStartNode()
            {
                List<IVariableNode> nodeBuffer = new(1);
                foreach (IVariable variable in editorGraph.GetVariables())
                {
                    if (variable.VariableKind != VariableKind.Input)
                        continue;
                    
                    variable.GetNodes(nodeBuffer);
                    
                    INode starterNode = nodeBuffer[0].GetOutputPort(0).FirstConnectedPort?.GetNode();
                    
                    if (starterNode != null)
                        graphData.startNodeID = nodeMap[starterNode];
                    
                    return;
                }
            }
        }
        
        private void CreateNodeMap(DialogueEditorGraph editorGraph, DialogueGraph targetGraph)
        {
            nodeMap = new Dictionary<INode, int>(editorGraph.NodeCount);
            portMap = new Dictionary<IPort, int>(editorGraph.NodeCount);
            
            List<IVariableNode> variableNodeBuffer = new(1); // Created outside to reduce heap allocations
            
            targetGraph.subgraphWireReferences = new Dictionary<Hash128, DialogueGraph.SubgraphWireReference>();
            
            int id = 0;
            
            // Prevents asset subgraphs from ending abruptly
            MapAssetSubGraphEndPoint();
            
            MapNodesRecursively(editorGraph.GetNodes());
            
            return;
            
            void MapAssetSubGraphEndPoint()
            {
                if (editorGraph.IsSubGraphInstance)
                    return;
                
                foreach (IVariable variable in editorGraph.GetVariables())
                {
                    if(variable.VariableKind != VariableKind.Output)
                        continue;
                    
                    variable.GetNodes(variableNodeBuffer);
                    
                    if (variableNodeBuffer.Count <= 0)
                        return;
                    
                    // Output node should
                    // be considered as a graceful exit
                    portMap.Add(variableNodeBuffer[0].GetInputPort(0), IDialogueGraphNode.GRACEFUL_END);
                }
                
            }
            
            void MapNodesRecursively(IEnumerable<INode> nodes)
            {
                foreach (INode node in nodes)
                {
                    if (nodeMap.ContainsKey(node)) // Can occur in case of sub graph force mappings
                        continue;
                    
                    if (node is EndNode)
                    {
                        portMap.Add(node.GetInputPort(0), IDialogueGraphNode.GRACEFUL_END);
                    }
                    
                    if (node is IDialogueGraphNode and not StartNode) // Start node is a special node which doesn't need an id
                    { 
                        portMap.Add(node.GetInputPortByName(IDialogueGraphNode.IN_EXECUTION), id);
                        nodeMap.Add(node, id++);
                    }
                    
                    if (node is ISubgraphNode subgraphNode)
                    {
                        if (IsAssetSubGraphNode(subgraphNode))
                            HandleAssetSubGraph(subgraphNode);
                        else
                            ExpandSubGraph(subgraphNode);
                    }
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
                
                portMap.Add(subgraphNode.GetInputPort(0), id);
                nodeMap.Add(subgraphNode, id++);
            }
            
            void ExpandSubGraph(ISubgraphNode subgraphNode)
            {
                Graph subgraph = subgraphNode.GetSubgraph();
                MapNodesRecursively(subgraph.GetNodes());
                
                //Support input node
                foreach (IVariable subgraphInputVariable in subgraph.GetVariables()) // There can be multiple input points and thus multiple start points
                {
                    if(subgraphInputVariable.VariableKind != VariableKind.Input)
                        continue;
                    
                    // Each input variable can only point to one corresponding input node
                    subgraphInputVariable.GetNodes(variableNodeBuffer); 
                    // There should always only be one here but no checks can be made yet due to API limits
                    
                    IPort variableNodeOutputPort = variableNodeBuffer[0].GetOutputPort(0);
                    INode subgraphStarterNode = variableNodeOutputPort.FirstConnectedPort.GetNode();
                    
                    IPort inputPort = null; // Should never be null according to API
                    foreach (IPort port in subgraphNode.GetInputPorts())
                    {
                        if(port.DisplayName != subgraphInputVariable.Name)
                            continue;
                        
                        inputPort = port;
                        break;
                    }
                    
                    portMap.Add(inputPort, nodeMap[subgraphStarterNode]);
                    
                    // Support wire references going into the subgraph node and back into the entry node inside the subgraph
                    DialogueGraph.SubgraphWireReference subgraphWireRef = new()
                    {
                        subgraphNodePort = inputPort.ID,
                        variableNodePort = variableNodeOutputPort.ID
                    };
                    IPort subgraphStarterNodeInputPort = subgraphStarterNode.GetInputPortByName(IDialogueGraphNode.IN_EXECUTION);
                    targetGraph.subgraphWireReferences.Add(subgraphStarterNodeInputPort.ID, subgraphWireRef);
                }
                
                //Support output node
                foreach (IVariable subgraphOutputVariable in subgraph.GetVariables())
                {
                    if(subgraphOutputVariable.VariableKind != VariableKind.Output)
                        continue;
                    
                    // Each output variable can only point to one corresponding output node
                    subgraphOutputVariable.GetNodes(variableNodeBuffer); // There should always only be one here but no checks can be made yet due to API limits
                    
                    // Node in the subgraph node (not the subgraph) points to inside the parent graph
                    IPort outputPort = null; // Should never be null according to API
                    foreach (IPort port in subgraphNode.GetOutputPorts())
                    {
                        if(port.DisplayName != subgraphOutputVariable.Name)
                            continue;
                        
                        outputPort = port;
                        break;
                    }
                    INode subgraphEndNode = outputPort.FirstConnectedPort.GetNode();
                    
                    int targetId;
                    if (subgraphEndNode is EndNode) // If the subgraph points to an end node in the origin graph, set the variable node inside the subgraph to point to a graceful end
                    {
                        targetId = IDialogueGraphNode.GRACEFUL_END; // Graceful end
                    }
                    else if(subgraphEndNode == null)
                    {
                        targetId = IDialogueGraphNode.INVALID_END;
                    }
                    else if (nodeMap.TryGetValue(subgraphEndNode, out int endNodeId))
                    {
                        targetId = endNodeId;
                    }
                    else
                    {
                        targetId = id;
                        
                        //Force map the node this subgraph is outputting to so that this subgraph may point to it
                        portMap.TryAdd(outputPort.FirstConnectedPort, targetId);
                        nodeMap.TryAdd(subgraphEndNode, id++);
                    }
                    
                    //Variable acts as node. Direct that node (from the subgraph) to the output node (of the parent graph)
                    IPort variableNodeInputPort = variableNodeBuffer[0].GetInputPort(0);
                    portMap.Add(variableNodeInputPort, targetId);
                    nodeMap.Add(variableNodeBuffer[0], targetId);
                    
                    // Support wire references going out of the subgraph and back into the parent graph
                    DialogueGraph.SubgraphWireReference subgraphWireRef = new()
                    {
                        subgraphNodePort = outputPort.ID,
                        variableNodePort = variableNodeInputPort.ID
                    };
                    targetGraph.subgraphWireReferences.Add(variableNodeInputPort.FirstConnectedPort.ID, subgraphWireRef);
                }
            }
        }
        
        private void ProcessNodes(DialogueEditorGraph editorGraph, DialogueGraph graph, AssetImportContext context)
        {
            var list = new List<(int id, DialogueNodeData data)>();
            List<ProvisionerData> provisionedData = new();
            HashSet<ScriptableObject> savedProvisions = new();
            
            ProcessNodesRecursively(editorGraph.GetNodes());
            
            list.Sort((a,b) => a.id.CompareTo(b.id));
            graph.nodes = new DialogueNodeData[list.Count];
            
            for (int i = 0; i < list.Count; i++)
            {
                graph.nodes[i] = list[i].data;
            }
            
            graph.provisionerData = provisionedData.ToArray();
            return;
            
            void ProcessNodesRecursively(IEnumerable<INode> nodes)
            {
                foreach (INode node in nodes)
                {
                    if (node is IScriptableObjectProvisionerNode provisionerNode)
                    {
                        ScriptableObject so = provisionerNode.GetScriptableObject();
                        if (savedProvisions.Add(so))
                            context.AddObjectToAsset("provision " + (savedProvisions.Count - 1), provisionerNode.GetScriptableObject());
                    }
                    else if (node is IRuntimeProvisioner runtimeProvisioner)
                    {
                        provisionedData.Add(runtimeProvisioner.ProcessNode(graph, portMap));
                    }
                
                    if (node is ISubgraphNode subgraphNode)
                    {
                        if (IsAssetSubGraphNode(subgraphNode))
                        {
                            DialogueNodeData subgraphData = ProcessAssetSubgraphNode(subgraphNode);
                            
                            if (subgraphData != null)
                                list.Add((nodeMap[subgraphNode], subgraphData));
                        }
                        else
                            ProcessNodesRecursively(subgraphNode.GetSubgraph().GetNodes());
                        
                        continue;
                    }
                    
                    if (node is not IDialogueGraphNode dialogueNode)
                        continue;
                    
                    DialogueNodeData data = dialogueNode.ProcessNode(graph, portMap);
                    
                    if (data != null)
                    {
                        data.nodeHash = node.ID;
                        list.Add((nodeMap[node], data));
                    }
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
            // No API to distinguish from asset subgraph to local subgraphs
            Type typeToQuery = subgraphNode.GetType().BaseType;
            PropertyInfo prop = typeToQuery.GetProperty("IsReferencingLocalSubgraph", BindingFlags.Instance | BindingFlags.Public);
            return !(bool)prop.GetValue(subgraphNode);
        }
    }
}
