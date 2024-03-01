using System.Reflection;
using LethalModDataLib.Attributes;

namespace LethalModDataLib.Interfaces;

/// <summary>
///     Interface for mod data keys (used in the ModDataValues dictionary)
/// </summary>
public interface IModDataKey
{
    /// <summary>
    ///     Name of the field or property.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Assembly qualified name of the field or property.
    /// </summary>
    public string? AssemblyQualifiedName { get; }

    /// <summary>
    ///     Assembly of the field or property.
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    ///     Instance of the class the field or property is in. Only null if the field or property is static.
    /// </summary>
    public object? Instance { get; }

    /// <summary>
    ///     Try to get the value of the field or property.
    /// </summary>
    /// <param name="value"> Output value of the field or property. </param>
    /// <returns> True if the value was successfully retrieved, false otherwise. </returns>
    public bool TryGetValue(out object? value);

    /// <summary>
    ///     Try to set the value of the field or property.
    /// </summary>
    /// <param name="value"> Value to set the field or property to. </param>
    /// <returns> True if the value was successfully set, false otherwise. </returns>
    public bool TrySetValue(object? value);

    /// <summary>
    ///     Get the ModDataAttribute of the field or property.
    /// </summary>
    /// <returns> ModDataAttribute of the field or property. </returns>
    public ModDataAttribute GetModDataAttribute();
}