using System;
using UnityEngine;
using System.Threading;

#if CLOGIC_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace CLogic.Dialogue
{
    [Serializable]
    public class BlockerNodeData : DialogueNodeData
    {
        [Flags]
        public enum BlockType
        {
            None = 0,
            AutoProgress = 1 << 0,
            Block = 1 << 1
        }

        public BlockType blockType;
    }

    public abstract class BlockerProcessor<T> : DialogueProcessor<T> where T : BlockerNodeData
    {
        protected CancellationTokenSource cancellationTokenSource;

        protected bool IsFinished { get; set; }

        protected override bool CanProgressNode(T nodeData, DialogueDirector director)
        {
            BlockerNodeData.BlockType blockType = nodeData.blockType;

            if (blockType.HasFlag(BlockerNodeData.BlockType.Block))
                return IsFinished;

            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();

            return true;
        }

        protected override void ProcessNode(T nodeData, DialogueDirector director)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();

            IsFinished = false;

            ProcessBlockerNode(cancellationTokenSource.Token, nodeData, director);

#if CLOGIC_UNITASK
            ProcessNodeAsync().Forget();

            async UniTask ProcessNodeAsync()
            {
                await ProcessBlockerNodeAsync(cancellationTokenSource.Token, nodeData, director);

                IsFinished = true;
                if (nodeData.blockType.HasFlag(BlockerNodeData.BlockType.AutoProgress))
                    director.GoToNextNode();
            }
#endif
        }

#if CLOGIC_UNITASK
        protected virtual void ProcessBlockerNode(CancellationToken context, T nodeData, DialogueDirector director) { }

        protected virtual UniTask ProcessBlockerNodeAsync(CancellationToken context, T nodeData, DialogueDirector director) => UniTask.CompletedTask;
#else
        public abstract void ProcessBlockerNode(CancellationToken context, T nodeData, DialogueDirector director);
#endif
    }
}
