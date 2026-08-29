using System;
using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
namespace CLogic.Dialogue.Extras
{
    public class TypeWriter : MonoBehaviour
    {
        public TextMeshProUGUI textDisplay;
        
        [TextArea]
        public string textToWrite;
        
        [TextArea]
        public string startText;
        
        [Header("Settings")]
        public float speedWordsPerMin = 120f;
        public bool autoStart = true;
        public bool delayEmptySpace = true;
        
        public bool IsWriting { get; private set; } = false;
        
        private Coroutine writingCoroutine;
        private Action currentFinishCallback;
        private string currentTargetText;
        
        public Action onWritingFinished;
        
        private void Start()
        {
            if (autoStart)
                StartWriting();
        }
        
        #region QOL Access
        public void StopWriting() => StopWriting(false, false);
        public void SkipAnimation() => StopWriting(true, true);
        
        public void StartWriting() => StartWriting(textToWrite);
        public void StartWriting(string text, Action finishCallback = null) => StartWriting(text, speedWordsPerMin, finishCallback, startText, delayEmptySpace);
        public void StartWriting(string text, float wordsPerMin, Action finishCallback) => StartWriting(text, wordsPerMin, finishCallback, null, true);
        #endregion
        
        public void StartWriting(string text, float wordsPerMin, Action finishCallback, [CanBeNull] string startText, bool delayEmptySpace)
        {
            if (writingCoroutine != null)
                StopCoroutine(writingCoroutine);
            
            currentFinishCallback = finishCallback;
            currentTargetText = text;
            writingCoroutine = StartCoroutine(WriterCoroutine(text, wordsPerMin, finishCallback, startText, delayEmptySpace));
        }
        
        public void StopWriting(bool finishText, bool invokeCallbacks)
        {
            if (finishText)
                textDisplay.text = currentTargetText;
            
            if (invokeCallbacks)
            {
                currentFinishCallback?.Invoke();
                onWritingFinished?.Invoke();
            }
            
            if (writingCoroutine != null)
                StopCoroutine(writingCoroutine);
            IsWriting = false;
        }
        
        private IEnumerator WriterCoroutine(string text, float wordsPerMinute, Action finishCallback = null, [CanBeNull] string startText = null, bool delayEmptySpace = true)
        {
            IsWriting = true;
            string currentText = startText ?? string.Empty;
            
            if (!string.IsNullOrEmpty(startText))
                text = text.TrimStart(startText.ToCharArray());
            
            float secondsPerCharacter = 60 / (wordsPerMinute * 5);
            bool isWritingTag = false;
            
            
            foreach (char c in text)
            {
                // ReSharper disable once ReplaceWithSingleAssignment.True
                bool shouldDelayCharacter = true;
                
                if (c == '<' && !delayEmptySpace)
                    shouldDelayCharacter = false;
                
                if (c == '<')
                {
                    isWritingTag = true;
                }
                else if (isWritingTag && c == '>')
                {
                    isWritingTag = false;
                }
                
                currentText += c;
                textDisplay.text = currentText;
                
                if (shouldDelayCharacter && !isWritingTag)
                    yield return new WaitForSeconds(secondsPerCharacter);
            }
            
            finishCallback?.Invoke();
            onWritingFinished?.Invoke();
            IsWriting = false;
        }
        
        private void Reset()
        {
            textDisplay = GetComponent<TextMeshProUGUI>();
        }
    }
}
