using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CLogic.Dialogue.Provisioner;

[assembly: InternalsVisibleTo("CLogic.Dialogue.Editor")]
namespace CLogic.Dialogue
{
    /// <summary>
    /// Runtime equivalent of the editor graph used for playing dialogues using the <see cref="DialogueDirector"/>
    /// </summary>
    public class DialogueGraph : ScriptableObject
    {
        public const int INVALID_END = -1;
        public const int GRACEFUL_END = -2;
        
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
    internal class SubGraphNodeData : DialogueNodeData
    {
        public DialogueGraph graph;
    }
}
