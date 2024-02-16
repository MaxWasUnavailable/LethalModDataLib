using LethalModDataLib.Attributes;

namespace LethalModDataLib.Interfaces;

/// <summary>
///     Interface for mod data keys (used in the ModDataValues dictionary)
/// </summary>
public interface IModDataKey
{
    public string Name { get; }
    public string? AssemblyQualifiedName { get; }
    public object? Instance { get; }
    public bool TryGetValue(out object? value);
    public bool TrySetValue(object? value);
    public ModDataAttribute GetModDataAttribute();
}