using System;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor.GraphVisualization;
using UnityEngine;
namespace CLogic.Dialogue.Provisioner
{
    public interface IDialogueProvisioner
    {
        public Type HandledType { get; }
        
        public T GetProvisionedData<T>(ProvisionerData nodeData);
    }
    
    [Serializable]
    public class ProvisionerData
    {
        //Key is node hash, value is name of the port to be provisioned
        [SerializeField]
        public Dictionary<Hash128, List<string>> linkedNodes;
    }
    
    public abstract class DialogueProvisioner<TNodeData, TProvisionData> : MonoBehaviour, IDialogueProcessor, IDialogueProvisioner where TNodeData : ProvisionerData
    {
        public Type HandledType => typeof(TNodeData);
        
        public virtual bool SupportsCaching { get; protected set; } = true;
        protected TProvisionData cache;
        
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
        
        protected abstract TProvisionData CreateProvisionedData(TNodeData nodeData);
        
        #region Interface Contracts
        public Type NodeType => HandledType;
        public bool CanProgressNode(DialogueNodeData nodeData, DialogueDirector director) => throw new Exception($"{nameof(DialogueProvisioner<TNodeData, TProvisionData>)} cannot process nodes");
        public void ProcessNode(DialogueNodeData nodeData, DialogueDirector director) => throw new Exception($"{nameof(DialogueProvisioner<TNodeData, TProvisionData>)} cannot process nodes");
        public void HandleCancellation(DialogueNodeData nodeData, DialogueDirector director) => throw new Exception($"{nameof(DialogueProvisioner<TNodeData, TProvisionData>)} cannot process nodes");
        public void VisualizeNode(Context ctx, DialogueNodeData nodeData) => throw new Exception($"{nameof(DialogueProvisioner<TNodeData, TProvisionData>)} cannot process nodes");
        #endregion
    }
}
