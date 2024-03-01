using System.Reflection;
using LethalModDataLib.Attributes;
using LethalModDataLib.Interfaces;

namespace LethalModDataLib.Models;

/// <summary>
///     Record used as key for ModDataValues dictionary
/// </summary>
public record FieldKey(FieldInfo FieldInfo, object? Instance = null) : IModDataKey
{
    /// <summary>
    ///     Name of the field.
    /// </summary>
    public string Name => FieldInfo.Name;

    /// <summary>
    ///     Assembly qualified name of the field.
    /// </summary>
    public string? AssemblyQualifiedName => FieldInfo.FieldType.AssemblyQualifiedName;

    /// <summary>
    ///     Assembly of the field.
    /// </summary>
    public Assembly Assembly => FieldInfo.FieldType.Assembly;

    /// <summary>
    ///     Tries to get the value of the field.
    /// </summary>
    /// <param name="value"> Out parameter to get the value of the field. </param>
    /// <returns> True if the value was successfully retrieved, false otherwise. </returns>
    public bool TryGetValue(out object? value)
    {
        value = FieldInfo.GetValue(Instance);
        return true;
    }

    /// <summary>
    ///     Tries to set the value of the field.
    /// </summary>
    /// <param name="value"> Value to set the field to. </param>
    /// <returns> True if the value was successfully set, false otherwise. </returns>
    public bool TrySetValue(object? value)
    {
        FieldInfo.SetValue(Instance, value);
        return true;
    }

    /// <summary>
    ///     Gets the ModDataAttribute of the field.
    /// </summary>
    /// <returns> The ModDataAttribute of the field. </returns>
    public ModDataAttribute GetModDataAttribute()
    {
        return FieldInfo.GetCustomAttribute<ModDataAttribute>()!;
    }
}