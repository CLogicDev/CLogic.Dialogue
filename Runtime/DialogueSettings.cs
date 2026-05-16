using System;
using CLogic.Utils.Shared;
namespace CLogic.Dialogue
{
    
    [Serializable]
    public class DefaultFeatures
    {
        public const string DEFINE_CONVERSATION_NODE = "ENABLE_CONVERSATION_NODE";
        public const string DEFINE_CHOICE_NODE = "ENABLE_CHOICE_NODE";
        public const string DEFINE_CHOICE_OPTION_NODE = "ENABLE_CHOICE_OPTION_NODE";
        
        public bool conversationNode, choiceNode, choiceOptionNode = true;
    }
    
    public class DialogueSettings : SettingsSo<DialogueSettings>
    {
        internal const string KEY = "dev.clogic.dialogue";
        
        protected override string AssetName { get; set; } = "DialogueSettings.asset";
        protected override string Key { get; set; } = KEY;

        // What features of the dialogue system should be enabled
        public DefaultFeatures features;
    }
}
