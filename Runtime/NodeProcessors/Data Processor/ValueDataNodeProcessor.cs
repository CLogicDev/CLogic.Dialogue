using System;
using CLogic.Runtime.DataSaving;
using CLogic.Dialogue;
namespace CLogic.Dialogue.DataSaving
{
    [Serializable]
    public class ValueData : SaveData<float>
    {}
    
    public class ValueDataNodeProcessor : DataNodeProcessor<float>
    {
        protected override void ProcessNode(SaveData<float> nodeData, DialogueDirector director)
        {
            base.ProcessNode(nodeData, director);
            
            GameSaver.SetData(nodeData.id, nodeData.data, nodeData.sectionId);
        }
    }
}
