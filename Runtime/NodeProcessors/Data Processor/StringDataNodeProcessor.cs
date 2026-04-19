using System;
using CLogic.Runtime.DataSaving;
using CLogic.Systems.DialogueSystem;
namespace CLogic.Dialogue.DataSaving
{
    [Serializable]
    public class StringData : SaveData<string>
    {}
    
    public class StringDataNodeProcessor : DataNodeProcessor<string>
    {
        public override void ProcessNode(SaveData<string> dialogueNode, DialogueDirector director)
        {
            base.ProcessNode(dialogueNode, director);
            
            GameSaver.SetData(dialogueNode.id, dialogueNode.data, dialogueNode.sectionId);
        }
    }
}
