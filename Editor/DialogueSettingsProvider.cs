using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using System.Collections.Generic;

namespace CLogic.Dialogue.Editor
{
    public class BuildTargetChangedHandler : IActiveBuildTargetChanged
    {
        public int callbackOrder => 0;

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget) => DialogueSettings.UpdateDefines(DialogueSettings.GetOrCreateSettings().features);
    }

    internal static class DialogueSettingsProvider
    {
        [SettingsProvider]
        private static SettingsProvider CreateProvider()
        {
            SettingsProvider provider = new("Project/CLogic/Dialogue Settings", SettingsScope.Project)
            {
                label = "Dialogue Settings",
                guiHandler = (searchContext) =>
                {
                    DialogueSettings settings = DialogueSettings.GetOrCreateSettings();
                    SerializedObject serializedObject = new(settings);
                    SerializedProperty featuresProperty = serializedObject.FindProperty(nameof(DialogueSettings.features));

                    featuresProperty.isExpanded = true;

                    EditorGUILayout.PropertyField(featuresProperty, true);

                    serializedObject.ApplyModifiedProperties();

                    if (GUI.changed)
                        DialogueSettings.UpdateDefines(settings.features);
                },
                keywords = new HashSet<string>(new[] { "Dialogue", "Settings" })
            };

            return provider;
        }
    }
}
