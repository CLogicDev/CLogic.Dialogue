using System;
using System.Collections.Generic;
using System.Linq;
using CLogic.Dialogue.Provisioner;
using Unity.GraphToolkit.Editor;
using UnityEngine;
namespace CLogic.Dialogue.Editor
{
    public interface IProvisionerNode
    {
        public const string OUT_PROVISION = "Provisioned Data";
        
        public void OnValidate(GraphLogger graphLogger);
        
        public Type HandledType { get; }

        public T GetProvisionedData<T>();
    }
    public interface IRuntimeProvisioner : IProvisionerNode
    {
        public ProvisionerData ProcessNode(DialogueGraph graph, Dictionary<IPort, int> portMap);
    }
    
    
    public interface IScriptableObjectProvisionerNode
    {
        public ScriptableObject GetScriptableObject();
    }
    
    [Serializable]
    public abstract class ProvisionerNode<T> : Node, IProvisionerNode
    {
        public Type HandledType => typeof(T);
        protected T cache;
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            context.AddOutputPort<T>(IProvisionerNode.OUT_PROVISION).WithDisplayName("").Build();
        }
        
        public T1 GetProvisionedData<T1>()
        {
            cache ??= CreateProvisionedData();

            if(cache is T1 casted)
                return casted;
            
            throw new InvalidCastException("Provisioned data is not of type " + typeof(T).Name);
        }
        
        protected abstract T CreateProvisionedData();
        
        public virtual void OnValidate(GraphLogger graphLogger) {  }
    }
    
    /// <typeparam name="TIn">Type of data that will be passed to the runtime processor</typeparam>
    /// <typeparam name="TOut">Type of data that will be provisioned to the asking node</typeparam>
    [Serializable]
    public abstract class RuntimeProvisionerNode<TIn, TOut> : Node, IRuntimeProvisioner where TOut : ProvisionerData, new()
    {
        public Type HandledType => typeof(TIn);
        
        public T1 GetProvisionedData<T1>() => default;
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            context.AddOutputPort<TIn>(IProvisionerNode.OUT_PROVISION).WithDisplayName("Provision").Build();
        }
        
        public ProvisionerData ProcessNode(DialogueGraph graph, Dictionary<IPort, int> portMap)
        {
            TOut data = new();
            
            List<IPort> provisionedPorts = new(1);
            
            GetOutputPortByName(IProvisionerNode.OUT_PROVISION).GetConnectedPorts(provisionedPorts);
            
            if (provisionedPorts.Any())
            {
                data.linkedNodes = new Dictionary<Hash128, List<string>>();
            }
            
            foreach (IPort provisionedPort in provisionedPorts)
            {
                Hash128 nodeID = provisionedPort.GetNode().ID;
                if(!data.linkedNodes.TryGetValue(nodeID, out List<string> linkedPortNames))
                    linkedPortNames = data.linkedNodes[nodeID] = new List<string>();
                
                linkedPortNames.Add(provisionedPort.Name);
            }
            
            ProcessNodeCore(ref data, graph, portMap);
            
            return data;
        }
        
        public abstract void ProcessNodeCore(ref TOut provisionData, DialogueGraph graph, Dictionary<IPort, int> portMap);
        
        public virtual void OnValidate(GraphLogger graphLogger) { }
    }
}
