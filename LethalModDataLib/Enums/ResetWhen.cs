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
    ///     Reset the field/property when a game over happens (quota not reached).
    /// </summary>
    OnGameOver = 1
}