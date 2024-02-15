using System;
using LethalModDataLib.Enums;

namespace LethalModDataLib.Attributes;

/// <summary>
///     Attribute to mark fields to be ignored by ModDataContainers' Save and Load methods.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ModDataIgnoreAttribute : Attribute
{
    /// <summary>
    ///     Attribute to mark fields or properties to be ignored by ModDataContainers' Save and Load methods.
    /// </summary>
    /// <param name="ignoreFlags"> Flags to determine when to ignore the field or property. </param>
    public ModDataIgnoreAttribute(IgnoreFlags ignoreFlags = IgnoreFlags.None)
    {
        IgnoreFlags = ignoreFlags;
    }

    /// <summary>
    ///     Enum value to mark fields or properties to be ignored by ModDataContainers' Save and Load methods.
    /// </summary>
    public IgnoreFlags IgnoreFlags { get; }
}