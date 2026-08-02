using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assemblies;
namespace CLogic.Dialogue
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class StaticProcessorAttribute : Attribute
    {
        
    }
    
    internal static class StaticProcessorScanner
    {
        public static IEnumerable<Type> GetStaticProcessorTypes()
        {
            return CurrentAssemblies.GetLoadedAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => !t.IsAbstract)
                    .Where(t => t.IsDefined(typeof(StaticProcessorAttribute), false))
                    .Where(t => typeof(IDialogueProcessor).IsAssignableFrom(t));
        }
    }
}
