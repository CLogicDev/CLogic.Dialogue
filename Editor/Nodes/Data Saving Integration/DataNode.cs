using System;
using System.Collections.Generic;
using CLogic.Systems.DialogueSystem;
using CLogic.Systems.DialogueSystem.Editor;
using Unity.GraphToolkit.Editor;
namespace CLogic.Dialogue.DataSaving.Editor
{
    [Serializable, UseWithContext(typeof(ActionNode))]
    public abstract class DataNode<T> : DialogueBlockNode<SaveData<T>>
    {
        public override bool SupportStartAction => false;
        public override bool SupportEndAction => false;
        
        private const string IN_SECTION = "SectionId";
        private const string IN_ID = "Id";
        private const string IN_DATA = "Data";
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            context.AddInputPort<string>(IN_SECTION).WithDisplayName("Section").Build();
            context.AddInputPort<string>(IN_ID).WithDisplayName("ID").Build();
            context.AddInputPort<T>(IN_DATA).WithDisplayName("Data").Build();
        }
        
        public override SaveData<T> ProcessNodeAsset(DialogueGraph graph, Dictionary<INode, int> nodeMap)
        {
            var data = new SaveData<T>();
            
            data.sectionId = GetPortValue<string>(GetInputPortByName(IN_SECTION));
            data.id = GetPortValue<string>(GetInputPortByName(IN_ID));
            data.data = GetPortValue<T>(GetInputPortByName(IN_DATA));
            
            return data;
        }
    }
    
    [Serializable, UseWithContext(typeof(ActionNode))]
    public class StringDataNode : DataNode<string>
    {}
    
    [Serializable, UseWithContext(typeof(ActionNode))]
    public class FlagDataNode : DataNode<bool>
    {}
    
    [Serializable, UseWithContext(typeof(ActionNode))]
    public class ValueDataNode : DataNode<float>
    {}
}
