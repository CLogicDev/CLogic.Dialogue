using System;
using CLogic.Runtime.DataSaving;
using CLogic.Dialogue;
namespace CLogic.Dialogue.DataSaving
{
    [Serializable]
    public class ValueData : SaveData<float>
    {}
    
    public class ValueDataProcessor : DataProcessor<float>
    {
        protected override void ProcessNode(SaveData<float> nodeData, DialogueDirector director)
        {
            GameSaver.SetData(nodeData.id, nodeData.data, nodeData.sectionId);
        }
    }
}
