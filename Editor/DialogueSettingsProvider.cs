using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
namespace CLogic.Dialogue.Editor
{
    public static class DialogueSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            SettingsProvider provider = new ("Project/CLogic/Dialogue Settings", SettingsScope.Project)
            {
                label = "Dialogue Settings",
                guiHandler = (searchContext) =>
                {
                    DialogueSettings settings = DialogueSettings.GetOrCreateSettings();
                    
                    SerializedObject so = new (settings);
                    
                    SerializedProperty featuresProperty = so.FindProperty(nameof(DialogueSettings.features));
                    featuresProperty.isExpanded = true;

                    EditorGUILayout.PropertyField(featuresProperty, true);
                    
                    so.ApplyModifiedProperties();

                    if (GUI.changed)
                    {
                        UpdateDefines(settings.features);
                    }
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Dialogue", "Settings" })
            };

            return provider;
        }

        private static void UpdateDefines(DefaultFeatures features)
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
