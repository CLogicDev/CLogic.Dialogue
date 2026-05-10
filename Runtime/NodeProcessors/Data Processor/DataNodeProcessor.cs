using System;
using CLogic.Runtime.DataSaving;
using CLogic.Dialogue;
using UnityEngine;
namespace CLogic.Dialogue.DataSaving
{
    [Serializable]
    public class SaveData<T> : BlockNodeData
    {
        public string sectionId;
        
        public string id;
        
        public T data;
    }
    
    public abstract class DataNodeProcessor<T> : DialogueNodeProcessor<SaveData<T>> { }
}
