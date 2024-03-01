using System.Reflection;
using LethalModDataLib.Attributes;
using LethalModDataLib.Interfaces;

namespace LethalModDataLib.Models;

/// <summary>
///     Record used as key for ModDataValues dictionary
/// </summary>
public record PropertyKey(PropertyInfo PropertyInfo, object? Instance = null) : IModDataKey
{
    /// <summary>
    ///     Name of the property.
    /// </summary>
    public string Name => PropertyInfo.Name;

    /// <summary>
    ///     Assembly qualified name of the property.
    /// </summary>
    public string? AssemblyQualifiedName => PropertyInfo.PropertyType.AssemblyQualifiedName;

    /// <summary>
    ///     Assembly of the property.
    /// </summary>
    public Assembly Assembly => PropertyInfo.PropertyType.Assembly;

    /// <summary>
    ///     Tries to get the value of the property.
    /// </summary>
    /// <param name="value"> Out parameter to get the value of the property. </param>
    /// <returns> True if the value was successfully retrieved, false otherwise. </returns>
    public bool TryGetValue(out object? value)
    {
        if (PropertyInfo.GetGetMethod(true) == null)
        {
            value = null;
            return false;
        }

        value = PropertyInfo.GetValue(Instance);
        return true;
    }

    /// <summary>
    ///     Tries to set the value of the property.
    /// </summary>
    /// <param name="value"> Value to set the property to. </param>
    /// <returns> True if the value was successfully set, false otherwise. </returns>
    public bool TrySetValue(object? value)
    {
        if (PropertyInfo.GetSetMethod(true) == null)
            return false;

        PropertyInfo.SetValue(Instance, value);
        return true;
    }

    /// <summary>
    ///     Gets the ModDataAttribute of the property.
    /// </summary>
    /// <returns> The ModDataAttribute of the property. </returns>
    public ModDataAttribute GetModDataAttribute()
    {
        return PropertyInfo.GetCustomAttribute<ModDataAttribute>()!;
    }
}