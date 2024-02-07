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
    /// Accept zero or more IgnoreFlag enum values.
    public ModDataIgnoreAttribute(IgnoreFlag ignoreFlags = IgnoreFlag.None)
    {
        IgnoreFlags = ignoreFlags;
    }
    
    /// <summary>
    ///     Enum value to mark fields or properties to be ignored by ModDataContainers' Save and Load methods.
    /// </summary>
    public IgnoreFlag IgnoreFlags { get; }
}