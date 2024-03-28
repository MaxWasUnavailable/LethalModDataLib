using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;

namespace LethalModDataLib.Models;

/// <summary>
///     Record used as value for ModDataFields and ModDataProperties dictionaries.
/// </summary>
public record ModDataValue(ModDataAttribute ModDataAttribute, string? KeySuffix = null, object? OriginalValue = null)
{
    /// <summary>
    ///     Base key for ES3.
    /// </summary>
    public string? BaseKey
    {
        get => ModDataAttribute.BaseKey;
        set => ModDataAttribute.BaseKey = value;
    }

    /// <summary>
    ///     When to save the field.
    /// </summary>
    public SaveWhen SaveWhen => ModDataAttribute.SaveWhen;

    /// <summary>
    ///     When to load the field.
    /// </summary>
    public LoadWhen LoadWhen => ModDataAttribute.LoadWhen;

    /// <summary>
    ///     When to reset the field.
    /// </summary>
    public ResetWhen ResetWhen => ModDataAttribute.ResetWhen;

    /// <summary>
    ///     Where to save the field.
    /// </summary>
    public SaveLocation SaveLocation => ModDataAttribute.SaveLocation;

    /// <summary>
    ///     Original value of the field.
    /// </summary>
    public object? OriginalValue { get; set; }
}