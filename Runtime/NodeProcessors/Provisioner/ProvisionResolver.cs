using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.Assemblies;
namespace CLogic.Dialogue.Provisioner
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ProvisionAttribute : Attribute
    {
        public string portName;
        public ProvisionAttribute(string inputPortName)
        {
            portName = inputPortName;
        }
    }
    
    [SingletonProcessor]
    internal partial class ProvisionResolver : DialoguePreProcessor<DialogueNodeData>
    {
        public override int Priority => -1000;
        
        private static Dictionary<Type, List<(ProvisionAttribute, FieldInfo)>> provisionedTypeCache = new();
        
        [OnCodeInitializing]
        private static void ScanForAttributes()
        {
            foreach (Type type in CurrentAssemblies.GetLoadedAssemblies().SelectMany(a => a.GetTypes()))
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attribute = field.GetCustomAttribute<ProvisionAttribute>();
                    
                    if (attribute == null)
                        continue;
                    
                    if(!provisionedTypeCache.TryGetValue(type, out List<(ProvisionAttribute, FieldInfo)> provisionedTypes))
                        provisionedTypes = provisionedTypeCache[type] = new List<(ProvisionAttribute, FieldInfo)>();
                    
                    provisionedTypes.Add((attribute, field));
                }
            }
        }
        
        protected override void PreProcess(DialogueNodeData nodeData, DialogueDirector director)
        {
            if(director.provisionerLookup.TryGetValue(nodeData.nodeHash, out ProvisionerData provisionerData))
                ResolveProvisionsFor(nodeData, provisionerData, director);
        }
        
        internal static void ResolveProvisionsFor(DialogueNodeData nodeData, ProvisionerData provisionerData, DialogueDirector director)
        {
            List<(ProvisionAttribute, FieldInfo)> dataToProvision = provisionedTypeCache[nodeData.GetType()];
            
            if(!director.TryGetProcessorForNode(provisionerData.GetType(), out IDialogueProcessor processor))
                throw new ArgumentException($"{provisionerData.GetType()} does not have a processor");
            
            if(processor is not IDialogueProvisioner provisioner)
                throw new ArgumentException($"{provisionerData.GetType()} is not a provisioner");
            
            foreach ((ProvisionAttribute, FieldInfo) attributeData in dataToProvision)
            {
                ProvisionAttribute attribute = attributeData.Item1;
                if(!provisionerData.linkedNodes[nodeData.nodeHash].Contains(attribute.portName))
                    continue;

                FieldInfo fieldInfo = attributeData.Item2;
                
                //TODO: Determine if generic should be removed
                object provision = provisioner.GetProvisionedData<object>(provisionerData);
                fieldInfo.SetValue(nodeData, provision);
                
                #if UNITY_EDITOR
                if(nodeData.provisionedPorts != null && nodeData.provisionedPorts.TryGetValue(attribute.portName, out Hash128 provisionedPort))
                    provisioner.PreviewProvision(director.CurrentContext, provisionedPort, provision);
                #endif
            }
        }
    }
}
