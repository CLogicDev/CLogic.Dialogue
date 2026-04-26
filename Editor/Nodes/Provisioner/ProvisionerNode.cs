using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;
namespace CLogic.Dialogue.Editor
{
    public interface IProvisionerNode
    {
        public void OnValidate(GraphLogger graphLogger);
    }
    
    public interface IScriptableObjectProvisionerNode
    {
        public ScriptableObject GetScriptableObject();
    }
    
    [Serializable]
    public abstract class ProvisionerNode<T> : Node, IProvisionerNode
    {
        private const string OUT_PROVISION = "Provisioned Data";
        
        protected T cache;
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            context.AddOutputPort<T>(OUT_PROVISION).WithDisplayName("").Build();
        }
        
        public T GetProvisionedData()
        {
            cache ??= GetProvisionedDataInternal();
            
            return cache;
        }
        
        public abstract T GetProvisionedDataInternal();
        
        public virtual void OnValidate(GraphLogger graphLogger) {  }
    }
}
