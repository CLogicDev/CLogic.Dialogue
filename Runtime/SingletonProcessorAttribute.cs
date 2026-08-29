using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assemblies;
namespace CLogic.Dialogue
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class SingletonProcessorAttribute : Attribute
    {
        internal static IEnumerable<Type> GetSingletonProcessorTypes()
        {
            return CurrentAssemblies.GetLoadedAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => !t.IsAbstract)
                    .Where(t => t.IsDefined(typeof(SingletonProcessorAttribute), false))
                    .Where(t => typeof(IDialogueProcessor).IsAssignableFrom(t));
        }
    }
}
