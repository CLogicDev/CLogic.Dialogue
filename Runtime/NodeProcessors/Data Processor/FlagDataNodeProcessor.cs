using System;
using CLogic.Runtime.DataSaving;
using CLogic.Dialogue;
namespace CLogic.Dialogue.DataSaving
{
    [Serializable]
    public class FlagData : SaveData<bool>
    {}
    
    public class FlagDataNodeProcessor : DataNodeProcessor<bool>
    {
        protected override void ProcessNode(SaveData<bool> nodeDate, DialogueDirector director)
        {
            base.ProcessNode(nodeDate, director);
            
            GameSaver.SetData(nodeDate.id, nodeDate.data, nodeDate.sectionId);
        }
    }
}
