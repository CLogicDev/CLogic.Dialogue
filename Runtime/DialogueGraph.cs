using System;
using UnityEngine;
using System.Collections.Generic;
using CLogic.Dialogue.Provisioner;

namespace CLogic.Dialogue
{
    public class DialogueGraph : ScriptableObject
    {
        [Serializable]
        public struct SubgraphWireReference
        {
            public Hash128 subgraphNodePort;
            public Hash128 variableNodePort;
        }
        
        [SerializeField]
        public Dictionary<Hash128, SubgraphWireReference> subgraphWireReferences;
        
        public Hash128 graphHash;
        public Hash128 entryPortHash;
        
        public int startNodeID = -1;
        
        [SerializeReference]
        public DialogueNodeData[] nodes;
        
        [SerializeReference]
        public ProvisionerData[] provisionerData;
    }
    
    [Serializable]
    public class SubGraphNodeData : DialogueNodeData
    {
        public DialogueGraph graph;
    }
}
