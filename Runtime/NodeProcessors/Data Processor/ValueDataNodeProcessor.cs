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
        protected override void ProcessNode(SaveData<float> nodeDate, DialogueDirector director)
        {
            base.ProcessNode(nodeDate, director);
            
            GameSaver.SetData(nodeDate.id, nodeDate.data, nodeDate.sectionId);
        }
    }
}
