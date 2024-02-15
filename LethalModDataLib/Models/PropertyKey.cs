using System.Reflection;
using LethalModDataLib.Attributes;
using LethalModDataLib.Interfaces;

namespace LethalModDataLib.Models;

/// <summary>
///     Record used as key for ModDataValues dictionary
/// </summary>
public record PropertyKey(PropertyInfo PropertyInfo, object? Instance = null) : IModDataKey
{
    public string Name => PropertyInfo.Name;
    public string? AssemblyQualifiedName => PropertyInfo.PropertyType.AssemblyQualifiedName;

    public bool TryGetValue(out object? value)
    {
        if (PropertyInfo.GetGetMethod() == null)
        {
            value = null;
            return false;
        }

        value = PropertyInfo.GetValue(Instance);
        return true;
    }

    public bool TrySetValue(object? value)
    {
        if (PropertyInfo.GetSetMethod() == null)
            return false;

        PropertyInfo.SetValue(Instance, value);
        return true;
    }

    public ModDataAttribute GetModDataAttribute()
    {
        return PropertyInfo.GetCustomAttribute<ModDataAttribute>()!;
    }
}