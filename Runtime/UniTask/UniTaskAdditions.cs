using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UTask = Cysharp.Threading.Tasks.UniTask;

namespace CLogic.Dialogue.UniTask
{
    public static class UniTaskAdditions
    {
        public static UTask ToUniTask(this DialogueHandle handle, CancellationToken cancellationToken = default)
        {
            cancellationToken.Register(static s => { (s as DialogueHandle).Cancel(); }, handle);
            
            return UTask.WaitUntil(handle, static h => h.IsFinished, cancellationToken: cancellationToken);
        }
    }
}
