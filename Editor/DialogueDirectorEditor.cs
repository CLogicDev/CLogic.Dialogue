using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using CLogic.Dialogue.Extras;

namespace CLogic.Dialogue.Editor
{
    [CanEditMultipleObjects, CustomEditor(typeof(DialogueDirector), true)]
    public class DialogueDirectorEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var playOnStartField = root.Q<PropertyField>("PropertyField:playOnStart");
            var dialogueGraphField = root.Q<PropertyField>("PropertyField:graph");

            dialogueGraphField.style.marginLeft = 20f;

            playOnStartField.RegisterCallback<ChangeEvent<bool>>((callback) => dialogueGraphField.style.display = callback.newValue ? DisplayStyle.Flex : DisplayStyle.None);

            return root;
        }

        [MenuItem("GameObject/CLogic/Dialogue/Director", priority = 10)]
        private static void CreateDirectorObject()
        {
            GameObject directorObject = new("DialogueDirector", typeof(DialogueDirector));

            Selection.activeGameObject = directorObject;
        }

        [MenuItem("GameObject/CLogic/Dialogue/Type Writer")]
        private static void CreateTypeWritterObject()
        {
            GameObject typeWritterObject = new("TypeWriterText", typeof(TypeWriter));

            Selection.activeGameObject = typeWritterObject;
        }
    }
}
