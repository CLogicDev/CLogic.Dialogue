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
        protected override void ProcessNode(SaveData<string> nodeDate, DialogueDirector director)
        {
            base.ProcessNode(nodeDate, director);
            
            GameSaver.SetData(nodeDate.id, nodeDate.data, nodeDate.sectionId);
        }
    }
}
