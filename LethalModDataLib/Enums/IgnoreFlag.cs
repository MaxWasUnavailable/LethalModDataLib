using System;

namespace LethalModDataLib.Enums;

/// <summary>
///     Enum for flags for the IgnoreAttribute.
/// </summary>
[Flags]
public enum IgnoreFlag
{
    /// <summary>
    ///     No flags. Completely ignore the field or property.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Ignore the field or property when saving.
    /// </summary>
    OnSave = 1,

    /// <summary>
    ///     Ignore the field or property when loading.
    /// </summary>
    OnLoad = 2,

    /// <summary>
    ///     Ignore the field or property if it is null.
    /// </summary>
    IfNull = 4,
    
    /// <summary>
    ///     Ignore the field or property if it is default.
    /// </summary>
    IfDefault = 8
}