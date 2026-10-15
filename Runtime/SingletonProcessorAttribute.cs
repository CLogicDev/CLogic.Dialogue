using System;
using System.Linq;
using UnityEngine.Assemblies;
using System.Collections.Generic;

namespace CLogic.Dialogue
{
    /// <summary>
    /// Marks a dialogue processor as a singleton, creating a single shared instance of it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class SingletonProcessorAttribute : Attribute
    {
        internal static IEnumerable<Type> GetSingletonProcessorTypes() => CurrentAssemblies.GetLoadedAssemblies().SelectMany((assembly) => assembly.GetTypes())
            .Where((type) => !type.IsAbstract).Where((type) => type.IsDefined(typeof(SingletonProcessorAttribute), false)).Where((type) => typeof(IDialogueProcessorBase).IsAssignableFrom(type));
    }
}
