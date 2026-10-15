using System;
using CLogic.Dialogue.Editor;
using Unity.GraphToolkit.Editor;
using System.Collections.Generic;

namespace CLogic.Dialogue.DataSaving.Editor
{
    [Serializable, UseWithContext(typeof(ActionNode))]
    public abstract class DataNode<T> : DialogueBlockNode<SaveData<T>>
    {
        private const string IN_SECTION = "SectionId";
        private const string IN_ID = "Id";
        private const string IN_DATA = "Data";

        public override bool SupportStartAction => false;
        public override bool SupportEndAction => false;

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);

            context.AddInputPort<string>(IN_SECTION).WithDisplayName("Section").Build();
            context.AddInputPort<string>(IN_ID).WithDisplayName("ID").Build();
            context.AddInputPort<T>(IN_DATA).WithDisplayName("Data").Build();
        }

        public override SaveData<T> ProcessNodeAsset(DialogueGraph graph, Dictionary<IPort, int> portMap)
        {
            var data = new SaveData<T>()
            {
                sectionId = GetPortValue<string>(GetInputPortByName(IN_SECTION)),
                id = GetPortValue<string>(GetInputPortByName(IN_ID)),
                data = GetPortValue<T>(GetInputPortByName(IN_DATA))
            };

            return data;
        }
    }

    [Serializable, UseWithContext(typeof(ActionNode)), Node("Data", "", "String Data")]
    public class StringDataNode : DataNode<string> { }

    [Serializable, UseWithContext(typeof(ActionNode)), Node("Data", "", "Flag Data")]
    public class FlagDataNode : DataNode<bool> { }

    [Serializable, UseWithContext(typeof(ActionNode)), Node("Data", "", "Value Data")]
    public class ValueDataNode : DataNode<float> { }
}
