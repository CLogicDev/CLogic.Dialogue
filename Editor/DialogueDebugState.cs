using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;

namespace CLogic.Dialogue.Editor
{
    [InitializeOnLoad, NoAutoStaticsCleanup]
    internal static class DialogueDebugState
    {
        public static List<DialogueDirector> ActiveDirectors = new();
        private static Dictionary<DialogueDirector, Hash128> ActiveNodeIDs = new();
        private static Dictionary<DialogueDirector, (Action OnUpdate, Action OnClear)> DirectorCallbacks = new();

        public static event Action OnActiveNodesChanged;

        static DialogueDebugState()
        {
            DialogueDirector.OnDirectorInitalized += OnDirectorInitalized;
            DialogueDirector.OnDirectorDestroyed += OnDirectorDestroyed;
        }

        public static bool IsNodeActive(Hash128 nodeID) => ActiveNodeIDs.ContainsValue(nodeID);

        private static void OnDirectorInitalized(DialogueDirector director) => AttachToDirector(director);

        private static void OnDirectorDestroyed(DialogueDirector director)
        {
            DetachFromDirector(director);
            ClearCurrentNode(director);
        }

        private static void AttachToDirector(DialogueDirector director)
        {
            if (DirectorCallbacks.ContainsKey(director))
                return;

            // Cache the callbacks for unsubscribing when detaching
            DirectorCallbacks.Add(director, (UpdateNode, ClearNode));

            director.OnDialogueProgress += UpdateNode;
            director.OnDialogueEnd += ClearNode;

            if (!ActiveDirectors.Contains(director))
                ActiveDirectors.Add(director);

            UpdateCurrentNode(director);

            void UpdateNode() => UpdateCurrentNode(director);
            void ClearNode() => ClearCurrentNode(director);
        }

        private static void DetachFromDirector(DialogueDirector director)
        {
            if (DirectorCallbacks.TryGetValue(director, out var callbacks))
            {
                director.OnDialogueProgress -= callbacks.OnUpdate;
                director.OnDialogueEnd -= callbacks.OnClear;

                DirectorCallbacks.Remove(director);
            }

            ActiveDirectors.Remove(director);
        }

        private static void UpdateCurrentNode(DialogueDirector director)
        {
            if (director == null || director.CurrentNode == null)
            {
                ActiveNodeIDs.Remove(director);
            }
            else
            {
                ActiveNodeIDs[director] = director.CurrentNode.nodeHash;
            }

            OnActiveNodesChanged?.Invoke();
        }

        private static void ClearCurrentNode(DialogueDirector director)
        {
            if (ActiveNodeIDs.Remove(director))
                OnActiveNodesChanged?.Invoke();
        }
    }
}
