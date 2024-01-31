namespace LethalModDataLib.Enums;

/// <summary>
///     Enum to specify when to load fields marked with <see cref="Attributes.ModDataAttribute" />.
/// </summary>
public enum LoadWhen
{
    /// <summary>
    ///     Loading is left to the modder to handle.
    /// </summary>
    Manual,

    /// <summary>
    ///     Load when the game loads a save.
    /// </summary>
    OnLoad
}