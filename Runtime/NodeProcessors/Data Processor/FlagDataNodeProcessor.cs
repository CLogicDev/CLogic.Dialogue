using System;
using CLogic.Runtime.DataSaving;
using CLogic.Systems.DialogueSystem;
namespace CLogic.Dialogue.DataSaving
{
    [Serializable]
    public class FlagData : SaveData<bool>
    {}
    
    public class FlagDataNodeProcessor : DataNodeProcessor<bool>
    {
        public override void ProcessNode(SaveData<bool> dialogueNode, DialogueDirector director)
        {
            base.ProcessNode(dialogueNode, director);
            
            GameSaver.SetData(dialogueNode.id, dialogueNode.data, dialogueNode.sectionId);
        }
    }
}
