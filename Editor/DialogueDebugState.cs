using System;
using UnityEngine;
using UnityEditor;
using Unity.Scripting.LifecycleManagement;
using Object = UnityEngine.Object;

namespace CLogic.Dialogue.Editor
{
    [InitializeOnLoad, NoAutoStaticsCleanup]
    internal static class DialogueDebugState
    {
        private static DialogueDirector director;

        public static Hash128 CurrentNodeID { get; private set; }

        public static event Action<Hash128> CurrentNodeChanged;

        static DialogueDebugState() => EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    AttachToDirector();
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    DetachFromDirector();
                    ClearCurrentNode();
                    break;
            }
        }

        private static void AttachToDirector()
        {
            DetachFromDirector();

            director = Object.FindAnyObjectByType<DialogueDirector>();

            if (director == null)
                return;

            director.OnDialogueStart += UpdateCurrentNode;
            director.OnDialogueProgress += UpdateCurrentNode;
            director.OnDialogueEnd += ClearCurrentNode;

            UpdateCurrentNode();
        }

        private static void DetachFromDirector()
        {
            if (director == null)
                return;

            director.OnDialogueStart -= UpdateCurrentNode;
            director.OnDialogueProgress -= UpdateCurrentNode;
            director.OnDialogueEnd -= ClearCurrentNode;

            director = null;
        }

        private static void UpdateCurrentNode()
        {
            if (director == null)
                return;

            SetCurrentNode(director.CurrentNode.nodeHash);
        }

        private static void ClearCurrentNode() => SetCurrentNode(default);

        private static void SetCurrentNode(Hash128 nodeID)
        {
            if (CurrentNodeID == nodeID)
                return;

            CurrentNodeID = nodeID;
            CurrentNodeChanged?.Invoke(CurrentNodeID);
        }
    }
}
