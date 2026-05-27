using System;
using CLogic.Runtime.DataSaving;
using CLogic.Dialogue;
namespace CLogic.Dialogue.DataSaving
{
    [Serializable]
    public class StringData : SaveData<string>
    {}
    
    public class StringDataNodeProcessor : DataNodeProcessor<string>
    {
        protected override void ProcessNode(SaveData<string> nodeData, DialogueDirector director)
        {
            base.ProcessNode(nodeData, director);
            
            GameSaver.SetData(nodeData.id, nodeData.data, nodeData.sectionId);
        }
    }
}
