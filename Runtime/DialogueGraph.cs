using System;
using UnityEngine;
using System.Collections.Generic;

namespace CLogic.Dialogue
{
    public class DialogueGraph : ScriptableObject
    {
        public Hash128 graphHash;
        public Hash128 entryPortHash;
        
        public int startNodeID = -1;
        
        [SerializeReference]
        public DialogueNodeData[] nodes;
    }
    
    [Serializable]
    public class SubGraphNodeData : DialogueNodeData
    {
        public DialogueGraph graph;
    }
}
