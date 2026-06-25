using System;
using Unity.GraphToolkit.Editor;
using UnityEngine;
namespace CLogic.Dialogue.Editor
{
    public interface IProvisionerNode
    {
        public void OnValidate(GraphLogger graphLogger);
        
        public Type HandledType { get; }

        public T GetProvisionedData<T>();
    }
    
    public interface IScriptableObjectProvisionerNode
    {
        public ScriptableObject GetScriptableObject();
    }
    
    [Serializable]
    public abstract class ProvisionerNode<T> : Node, IProvisionerNode
    {
        private const string OUT_PROVISION = "Provisioned Data";
        
        public Type HandledType => typeof(T);
        protected T cache;
        
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            base.OnDefinePorts(context);
            
            context.AddOutputPort<T>(OUT_PROVISION).WithDisplayName("").Build();
        }
        
        public T1 GetProvisionedData<T1>()
        {
            cache ??= GetProvisionedDataInternal();

            if(cache is T1 casted)
                return casted;
            
            throw new InvalidCastException("Provisioned data is not of type " + typeof(T).Name);
        }
        
        protected abstract T GetProvisionedDataInternal();
        
        public virtual void OnValidate(GraphLogger graphLogger) {  }
    }
}
