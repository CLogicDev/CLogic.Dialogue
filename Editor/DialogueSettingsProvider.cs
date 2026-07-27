using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
namespace CLogic.Dialogue.Editor
{
    public class BuildTargetChangedHandler : IActiveBuildTargetChanged
    {
        public int callbackOrder => 0;

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            DialogueSettings.UpdateDefines(DialogueSettings.GetOrCreateSettings().features);
        }
    }
    
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
                    var settings = DialogueSettings.GetOrCreateSettings();
                    
                    SerializedObject so = new (settings);
                    
                    SerializedProperty featuresProperty = so.FindProperty(nameof(DialogueSettings.features));
                    featuresProperty.isExpanded = true;

                    EditorGUILayout.PropertyField(featuresProperty, true);
                    
                    so.ApplyModifiedProperties();

                    if (GUI.changed)
                    {
                        DialogueSettings.UpdateDefines(settings.features);
                    }
                },
                keywords = new HashSet<string>(new[] { "Dialogue", "Settings" })
            };

            return provider;
        }
    }
}
