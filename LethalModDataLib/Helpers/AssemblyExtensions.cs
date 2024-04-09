using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LethalModDataLib.Helpers;

/// Copyright notice:
/// 
/// Taken from OpenMod's source code, which is licensed under the MIT License.
/// https://github.com/openmod/openmod/blob/553d01717bfa9d0fb3db8aadcc39e7303f6b61ed/LICENSE
/// 
/// Relevant source code:
/// https://github.com/openmod/openmod/blob/main/framework/OpenMod.Common/Helpers/AssemblyExtensions.cs#L28
/// <summary>
///     Extension methods for the <see cref="Assembly" /> class.
/// </summary>
public static class AssemblyExtensions
{
    /// <summary>
    ///     Safely returns the set of loadable types from an assembly.
    /// </summary>
    /// <param name="assembly">The <see cref="Assembly" /> from which to load types.</param>
    /// <returns>
    ///     The set of types from the <paramref name="assembly" />, or the subset
    ///     of types that could be loaded if there was any error.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="assembly" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Avoid using this method, unless you don't care about missing types
    /// </remarks>
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null);
        }
    }
}