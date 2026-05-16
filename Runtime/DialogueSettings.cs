using System;
using System.Collections.Generic;
using System.Linq;
using CLogic.Utils.Shared;
using UnityEditor;
using UnityEditor.Build;
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
        
        public static void UpdateDefines(DefaultFeatures features)
        {
            NamedBuildTarget target = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string currentDefines = PlayerSettings.GetScriptingDefineSymbols(target);
            List<string> listedDefines = currentDefines
                .Split(';')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
            
            AddOrRemove(DefaultFeatures.DEFINE_CONVERSATION_NODE, features.conversationNode);
            AddOrRemove(DefaultFeatures.DEFINE_CHOICE_NODE, features.choiceNode);
            AddOrRemove(DefaultFeatures.DEFINE_CHOICE_OPTION_NODE, features.choiceOptionNode);
            
            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", listedDefines));
            
            return;

            void AddOrRemove(string define, bool exists)
            {
                if(exists)
                    listedDefines.Add(define);
                else
                    listedDefines.Remove(define);
            }
        }
    }
}
