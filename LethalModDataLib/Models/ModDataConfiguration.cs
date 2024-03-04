using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;

namespace LethalModDataLib.Models;

/// <summary>
///     Configuration used as argument for <see cref="ModDataAttribute" />.
/// </summary>
public record ModDataConfiguration(
    SaveWhen SaveWhen,
    LoadWhen LoadWhen,
    SaveLocation SaveLocation,
    string? BaseKey = null)
{
    /// <summary>
    ///     A general-purpose default configuration.
    /// </summary>
    public static ModDataConfiguration Default { get; } =
        new(SaveWhen.OnSave, LoadWhen.OnLoad, SaveLocation.CurrentSave);

    /// <summary>
    ///     A configuration for purely manual saving and loading using the attribute.
    /// </summary>
    public static ModDataConfiguration DefaultManual { get; } =
        new(SaveWhen.Manual, LoadWhen.Manual, SaveLocation.CurrentSave);
}