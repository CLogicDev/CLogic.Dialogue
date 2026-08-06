using System;
using System.Collections.Generic;
using UnityEngine;
namespace CLogic.Dialogue.Provisioner
{
    public interface IDialogueProvisioner
    {
        public Type HandledType { get; }
        
        public T GetProvisionedData<T>(ProvisionerData nodeData);
        
        #if UNITY_EDITOR
        public void PreviewProvision(Unity.GraphToolkit.Editor.GraphVisualization.Context ctx, Hash128 provisionedPortHash, object provision);
        #endif
    }
    
    [Serializable]
    public class ProvisionerData
    {
        //Key is node hash, value is name of the port to be provisioned
        [SerializeField]
        public Dictionary<Hash128, List<string>> linkedNodes;
    }
    
    /// <typeparam name="TNodeData">Type of data that will be passed to the runtime processor</typeparam>
    /// <typeparam name="TProvision">Type of data that will be provisioned to the asking node</typeparam>
    public abstract class DialogueProvisioner<TNodeData, TProvision> : MonoBehaviour, IDialogueProcessor, IDialogueProvisioner where TNodeData : ProvisionerData
    {
        public Type HandledType => typeof(TNodeData);
        
        public virtual bool SupportsCaching { get; protected set; } = true;
        protected TProvision cache;
        
        public T1 GetProvisionedData<T1>(ProvisionerData nodeData)
        {
            if (SupportsCaching)
                cache ??= CreateProvisionedData((TNodeData)nodeData);
            else
                cache = CreateProvisionedData((TNodeData)nodeData);
            
            if(cache is T1 casted)
                return casted;
            
            throw new InvalidCastException("Provisioned data is not of type " + typeof(TNodeData).Name);
        }
        
        protected abstract TProvision CreateProvisionedData(TNodeData nodeData);
        
        #if UNITY_EDITOR
        public virtual void PreviewProvision(Unity.GraphToolkit.Editor.GraphVisualization.Context ctx, Hash128 provisionedPortHash, object provision)
        {
            ctx.GetPortReference(provisionedPortHash).SetPreview(provision.ToString());
        }
        #endif
        
        #region Interface Contracts
        public Type NodeType => HandledType;
        public bool CanProgressNode(DialogueNodeData nodeData, DialogueDirector director) => throw new Exception($"{nameof(DialogueProvisioner<TNodeData, TProvision>)} cannot process nodes");
        public void ProcessNode(DialogueNodeData nodeData, DialogueDirector director) => throw new Exception($"{nameof(DialogueProvisioner<TNodeData, TProvision>)} cannot process nodes");
        public void HandleCancellation(DialogueNodeData nodeData, DialogueDirector director) => throw new Exception($"{nameof(DialogueProvisioner<TNodeData, TProvision>)} cannot process nodes");
        #if UNITY_EDITOR
        public void VisualizeNode(Unity.GraphToolkit.Editor.GraphVisualization.Context ctx, DialogueNodeData nodeData) => throw new Exception($"{nameof(DialogueProvisioner<TNodeData, TProvision>)} cannot process nodes");
        #endif
        #endregion
    }
}
