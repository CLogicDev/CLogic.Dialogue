using UnityEngine;
using UnityEngine.UIElements;
using Unity.GraphToolkit.Editor;

namespace CLogic.Dialogue.Editor
{
    public class DialogueNodeView<T> : NodeView<T> where T : Node
    {
        private VisualElement dottedBorder;
        private VisualElement externalNodeBorder;

        public override void OnViewBuilt()
        {
            Material borderMaterial = new(Shader.Find("CLogic/DialogueSystem/S_DottedBorder"))
            {
                hideFlags = HideFlags.HideAndDontSave // Prevents the material from being removed when exiting play mode
            };

            dottedBorder = new VisualElement
            {
                name = "DottedBorder",
                pickingMode = PickingMode.Ignore,
                style =
                {
                    flexGrow = 1f,
                    borderTopColor = Color.white,
                    borderBottomColor = Color.white,
                    borderLeftColor = Color.white,
                    borderRightColor = Color.white,
                    borderTopWidth = 5f,
                    borderBottomWidth = 5f,
                    borderLeftWidth = 5f,
                    borderRightWidth = 5f,
                    borderBottomLeftRadius = 15f,
                    borderBottomRightRadius = 15f,
                    borderTopLeftRadius = 15f,
                    borderTopRightRadius = 15f,
                    marginBottom = 10f,
                    marginLeft = 10f,
                    marginRight = 10f,
                    marginTop = 10f,
                    zIndex = -10,
                    display = DisplayStyle.None,
                    unityMaterial = borderMaterial
                }
            };

            externalNodeBorder = View.Root.Q<VisualElement>(className: "ge-graph-element__dynamic-border");
            externalNodeBorder.Add(dottedBorder);
        }

        public override void OnViewAttached()
        {
            DialogueDebugState.CurrentNodeChanged += UpdateHighlight;

            UpdateHighlight(DialogueDebugState.CurrentNodeID);
        }

        public override void OnViewDetached() => DialogueDebugState.CurrentNodeChanged -= UpdateHighlight;

        public override void OnCullingChanged(bool cullingEnabled)
        {
            if (externalNodeBorder == null || dottedBorder == null)
                return;

            if (cullingEnabled)
            {
                if (externalNodeBorder.Contains(dottedBorder))
                    externalNodeBorder.Remove(dottedBorder);
            }
            else
            {
                if (!externalNodeBorder.Contains(dottedBorder))
                    externalNodeBorder.Add(dottedBorder);
            }
        }

        private void UpdateHighlight(Hash128 nodeHash) => dottedBorder.style.display = nodeHash == Node.ID ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
