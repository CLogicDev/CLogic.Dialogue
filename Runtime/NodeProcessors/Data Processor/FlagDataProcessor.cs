using System;
using CLogic.Runtime.DataSaving;
using CLogic.Dialogue;
namespace CLogic.Dialogue.DataSaving
{
    [Serializable]
    public class FlagData : SaveData<bool>
    {}
    
    public class FlagDataProcessor : DataProcessor<bool>
    {
        protected override void ProcessNode(SaveData<bool> nodeData, DialogueDirector director)
        {
            GameSaver.SetData(nodeData.id, nodeData.data, nodeData.sectionId);
        }
    }
}
