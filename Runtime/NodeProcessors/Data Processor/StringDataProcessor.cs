using System;
using CLogic.Runtime.DataSaving;
using CLogic.Dialogue;
namespace CLogic.Dialogue.DataSaving
{
    [Serializable]
    public class StringData : SaveData<string>
    {}
    
    public class StringDataProcessor : DataProcessor<string>
    {
        protected override void ProcessNode(SaveData<string> nodeData, DialogueDirector director)
        {
            GameSaver.SetData(nodeData.id, nodeData.data, nodeData.sectionId);
        }
    }
}
