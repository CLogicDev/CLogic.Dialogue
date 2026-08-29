using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor;
namespace CLogic.Dialogue.Editor
{
    [Serializable]
    public abstract class BlockerNode<T> : DialogueNode<T> where T : BlockerNodeData, new()
    {
        public const string IN_BLOCK_TYPE = "blockType";
        
        protected override void DefineDialoguePorts(IPortDefinitionContext context)
        {
            CreateDefaultExecutionPorts(context);
            context.AddInputPort<BlockerNodeData.BlockType>(IN_BLOCK_TYPE).WithDisplayName("Block Type").WithDefaultValue(BlockerNodeData.BlockType.Block).Build();
        }
        public override T ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portMap)
        {
            T data = new()
            {
                blockType = GetPortValue<BlockerNodeData.BlockType>(GetInputPortByName(IN_BLOCK_TYPE))
            };
            
            CreateNodeLink(data, portMap);
            return data;
        }
    }
}
