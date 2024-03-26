using System;

namespace LethalModDataLib.Enums;

/// <summary>
///     Enum to specify when to load fields/properties marked with <see cref="Attributes.ModDataAttribute" />.
/// </summary>
[Flags]
public enum LoadWhen
{
    /// <summary>
    ///     Loading is left to the modder to handle.
    /// </summary>
    Manual = 0,

    /// <summary>
    ///     Load when the game loads a save.
    /// </summary>
    OnLoad = 1,

    /// <summary>
    ///     Load as soon as the field/property is registered by the API.
    /// </summary>
    OnRegister = 2
}