using System;

namespace LethalModDataLib.Enums;

/// <summary>
///     Enum to specify when to reset fields/properties marked with <see cref="Attributes.ModDataAttribute" />.
/// </summary>
[Flags]
public enum ResetWhen
{
    /// <summary>
    ///     No automatic reset. Modder is responsible for resetting the field/property when needed.
    /// </summary>
    Manual = 0,

    /// <summary>
    ///     Reset when we load or create a new save (before loading mod data).
    /// </summary>
    OnLoad = 1
}