using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace CLogic.Dialogue.Editor
{
    [FilePath("ProjectSettings/CLogic/DialogueSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class DialogueSettings : ScriptableSingleton<DialogueSettings>
    {
        public const string DEFINE_CHOICE_OPTION_NODE = "ENABLE_CHOICE_OPTION_NODE";
        public const string DEFINE_CONVERSATION_NODE = "ENABLE_CONVERSATION_NODE";
        public const string DEFINE_CHOICE_NODE = "ENABLE_CHOICE_NODE";

        [SerializeField] private bool choiceNode = true;
        [SerializeField] private bool conversationNode = true;
        [SerializeField] private bool choiceOptionNode = true;

        internal void UpdateDefines()
        {
            NamedBuildTarget target = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string currentDefines = PlayerSettings.GetScriptingDefineSymbols(target);

            List<string> listedDefines = currentDefines.Split(';').Where((define) => !string.IsNullOrWhiteSpace(define)).ToList();

            ToggleDefine(DEFINE_CONVERSATION_NODE, conversationNode);
            ToggleDefine(DEFINE_CHOICE_NODE, choiceNode);
            ToggleDefine(DEFINE_CHOICE_OPTION_NODE, choiceOptionNode);

            string newDefines = string.Join(";", listedDefines);

            // Avoid triggering a recompile when nothing has changed.
            if (newDefines != currentDefines)
                PlayerSettings.SetScriptingDefineSymbols(target, newDefines);

            void ToggleDefine(string define, bool exists)
            {
                if (exists)
                {
                    if (!listedDefines.Contains(define))
                        listedDefines.Add(define);
                }
                else
                {
                    listedDefines.Remove(define);
                }
            }
        }

        internal void SaveSettings() => Save(true);
    }

    internal class DialogueSettingsProvider : SettingsProvider
    {
        internal DialogueSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            SerializedObject serializedObject = new(DialogueSettings.instance);

            Label header = new("Dialogue Settings")
            {
                style =
                {
                    fontSize = 19f,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    justifyContent = Justify.FlexEnd,
                    height = 19f,
                    marginTop = 1f,
                    marginLeft = 9f,
                    marginBottom = 12f
                }
            };

            InspectorElement inspectorElement = new(serializedObject);

            inspectorElement.Q<ObjectField>("unity-input-m_Script").parent.RemoveFromHierarchy(); // Remove the auto-generated script field

            inspectorElement.RegisterCallback<ChangeEvent<bool>>((callback) => DialogueSettings.instance.UpdateDefines());

            rootElement.Add(header);
            rootElement.Add(inspectorElement);
        }

        public override void OnDeactivate() => DialogueSettings.instance.SaveSettings();

        [SettingsProvider]
        internal static SettingsProvider CreateSettingsProvider() => new DialogueSettingsProvider("Project/CLogic/Dialogue Settings", SettingsScope.Project)
        {
            keywords = new HashSet<string>(new[] { "CLogic", "Dialogue", "System", "Settings" })
        };
    }

    internal class BuildTargetChangedHandler : IActiveBuildTargetChanged
    {
        public int callbackOrder => 0;

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget) => DialogueSettings.instance.UpdateDefines();
    }
}
