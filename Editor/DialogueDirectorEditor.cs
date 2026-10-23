using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using CLogic.Dialogue.Extras;
using System.Collections.Generic;

namespace CLogic.Dialogue.Editor
{
    [CanEditMultipleObjects, CustomEditor(typeof(DialogueDirector), true)]
    public class DialogueDirectorEditor : UnityEditor.Editor
    {
        private const string DROPDOWN_DEFAULT_VALUE = "(select processor)";

        private Dictionary<string, string> processorTypeMap = new();

        private void OnEnable() => processorTypeMap = GetProcessors();

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new();
            DropdownField processorDropdown = new("Add Processors", processorTypeMap.Keys.ToList(), 0);

            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var playOnStartField = root.Q<PropertyField>("PropertyField:playOnStart");
            var dialogueGraphField = root.Q<PropertyField>("PropertyField:graph");

            processorDropdown.style.marginTop = 10f;
            dialogueGraphField.style.marginLeft = 20f;

            processorDropdown.AddToClassList(DropdownField.alignedFieldUssClassName);

            playOnStartField.RegisterCallback<ChangeEvent<bool>, PropertyField>( (callback, graphField) => graphField.style.display = callback.newValue ? DisplayStyle.Flex : DisplayStyle.None, dialogueGraphField);
            processorDropdown.RegisterCallback<ChangeEvent<string>, Dictionary<string, string>>((callback, typeMap) =>
            {
                if (callback.newValue == DROPDOWN_DEFAULT_VALUE)
                    return;
                
                foreach (var target in targets)
                {
                    var targetComponent = target as Component;
                    
                    var componentType = Type.GetType(typeMap[callback.newValue]);
                    
                    if (targetComponent.GetComponent(componentType) == null) // Disallow adding the component if it's already present
                        targetComponent.gameObject.AddComponent(componentType);
                }
                
                processorDropdown.SetValueWithoutNotify(DROPDOWN_DEFAULT_VALUE);
            }, processorTypeMap);

            root.Add(processorDropdown);

            return root;
        }

        [MenuItem("GameObject/CLogic/Dialogue/Director", priority = 10)]
        private static void CreateDirectorObject()
        {
            GameObject directorObject = new("DialogueDirector", typeof(DialogueDirector));

            Selection.activeGameObject = directorObject;
        }

        [MenuItem("GameObject/CLogic/Dialogue/Type Writer")]
        private static void CreateTypeWriterObject()
        {
            GameObject typeWriterObject = new("TypeWriterText", typeof(TypeWriter));

            Selection.activeGameObject = typeWriterObject;
        }

        private Dictionary<string, string> GetProcessors()
        {
            Dictionary<string, string> processorList = new() { { DROPDOWN_DEFAULT_VALUE, null } };
            List<string> processorsToExclude = GetExistingProcessors();

            foreach (Type type in TypeCache.GetTypesDerivedFrom<IDialogueProcessorBase>())
            {
                if (processorsToExclude != null && processorsToExclude.Contains(type.Name)) // Exclude processors that are already added from the dropdown
                    continue;

                if (!type.IsGenericType && !type.IsInterface && type != typeof(DialogueProcessor) && type.GetCustomAttribute<SingletonProcessorAttribute>() == null)
                    processorList.Add(type.Name, type.AssemblyQualifiedName);
            }

            return processorList;
        }

        private List<string> GetExistingProcessors()
        {
            // Can't easily tell what processors are or not on multiple targets so we will not attempt to exclude them, they won't be added twice anyway.
            if (targets.Length > 1)
                return null;

            List<string> existingProcessors = new();

            var targetComponent = target as Component;

            foreach (var processor in targetComponent.GetComponents<IDialogueProcessorBase>())
                existingProcessors.Add(processor.GetType().Name);

            return existingProcessors;
        }
    }
}
