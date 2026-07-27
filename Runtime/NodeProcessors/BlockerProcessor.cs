using System;
using System.Threading;
#if CLOGIC_UNITASK
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;
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
        protected CancellationTokenSource cts;
        
        private bool isFinished;
        protected bool IsFinished
        {
            get => isFinished;
            set
            {
                isFinished = value;
            }
        }
        
        protected override bool CanProgressNode(T nodeData, DialogueDirector director)
        {
            BlockerNodeData.BlockType blockType = nodeData.blockType;
            
            if (blockType.HasFlag(BlockerNodeData.BlockType.Block))
                return IsFinished;
            
            cts?.Cancel();
            cts?.Dispose();
            return true;
        }
        
        protected override void ProcessNode(T nodeData, DialogueDirector director)
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();
            
            IsFinished = false;
            
            ProcessBlockerNode(cts.Token, nodeData, director);
            
            #if CLOGIC_UNITASK
            ProcessNodeAsync().Forget();
            
            return;
            
            async UniTask ProcessNodeAsync()
            {
                await ProcessBlockerNodeAsync(cts.Token, nodeData, director);
                
                IsFinished = true;
                if (nodeData.blockType.HasFlag(BlockerNodeData.BlockType.AutoProgress))
                    director.GoToNextNode();
            }
            #endif
        }
        
        #if CLOGIC_UNITASK
        protected virtual void ProcessBlockerNode(CancellationToken ctx, T nodeData, DialogueDirector director)
        {}
        protected virtual UniTask ProcessBlockerNodeAsync(CancellationToken ctx, T nodeData, DialogueDirector director) => UniTask.CompletedTask;
        #else
        public abstract void ProcessBlockerNode(CancellationToken ctx, T nodeData, DialogueDirector director);
        #endif
    }
}
