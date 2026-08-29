using UnityEngine;
using UnityEngine.InputSystem;

namespace CLogic.Dialogue
{
    public class DialogueProgressor : MonoBehaviour
    {
        [SerializeField]
        private DialogueDirector dialogueDirector;
        [SerializeField]
        private InputActionReference dialogueProgressInput;
        
        private void OnEnable()
        {
            dialogueProgressInput.action.Enable();
            dialogueProgressInput.action.performed += OnDialogueProgress;
        }
        private void OnDisable() => dialogueProgressInput.action.performed -= OnDialogueProgress;
        
        private void OnDialogueProgress(InputAction.CallbackContext context)
        {
            dialogueDirector.GoToNextNode();
        }
    }
}
