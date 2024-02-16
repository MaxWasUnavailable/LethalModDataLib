using System.Reflection;
using LethalModDataLib.Attributes;
using LethalModDataLib.Interfaces;

namespace LethalModDataLib.Models;

/// <summary>
///     Record used as key for ModDataValues dictionary
/// </summary>
public record FieldKey(FieldInfo FieldInfo, object? Instance = null) : IModDataKey
{
    public string Name => FieldInfo.Name;
    public string? AssemblyQualifiedName => FieldInfo.FieldType.AssemblyQualifiedName;
    public Assembly Assembly => FieldInfo.FieldType.Assembly;

    public bool TryGetValue(out object? value)
    {
        value = FieldInfo.GetValue(Instance);
        return true;
    }

    public bool TrySetValue(object? value)
    {
        FieldInfo.SetValue(Instance, value);
        return true;
    }

    public ModDataAttribute GetModDataAttribute()
    {
        return FieldInfo.GetCustomAttribute<ModDataAttribute>()!;
    }
}