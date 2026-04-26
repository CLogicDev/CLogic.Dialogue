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
        public override void ProcessNode(SaveData<float> dialogueNode, DialogueDirector director)
        {
            base.ProcessNode(dialogueNode, director);
            
            GameSaver.SetData(dialogueNode.id, dialogueNode.data, dialogueNode.sectionId);
        }
    }
}
