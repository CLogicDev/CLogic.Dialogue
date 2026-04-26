using System;
using TMPro;
using UnityEngine;
using System.Collections;

namespace CLogic.Dialogue
{
    [RequireComponent(typeof(TMP_Text)), Obsolete]
    public class TextIterator : MonoBehaviour
    {
        [SerializeField]
        private float defaultTypeSpeed;
        
        private float typeSpeed;
        private string currentText;
        
        private TMP_Text textUI;
        private Coroutine textCoroutine;
        
        public bool IsIterating { get; private set; }
        
        private void Awake()
        {
            typeSpeed = defaultTypeSpeed;
            textUI = GetComponent<TMP_Text>();
        }
        
        public void StartTextIteration(string text, float? overrideTypeSpeed = null)
        {
            if (IsIterating)
                return;
            
            currentText = text;
            IsIterating = true;
            textUI.text = string.Empty;
            typeSpeed = overrideTypeSpeed ?? defaultTypeSpeed;
            
            textCoroutine = StartCoroutine(TextIteratorRoutine());
        }
        
        public void BrakeTextIteration()
        {
            IsIterating = false;
            textUI.text = currentText;
            
            StopCoroutine(textCoroutine);
        }
        
        private IEnumerator TextIteratorRoutine()
        {
            WaitForSecondsRealtime waitTime = new(typeSpeed);
            
            foreach (char letter in currentText.ToCharArray())
            {
                yield return waitTime;
                textUI.text += letter;
            }
            
            IsIterating = false;
        }
    }
}
