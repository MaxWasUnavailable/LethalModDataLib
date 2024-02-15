using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;

namespace LethalModDataLib.Models;

/// <summary>
///     Record used as value for ModDataFields & ModDataProperties dictionaries.
/// </summary>
public record ModDataValue(ModDataAttribute ModDataAttribute, string? KeySuffix = null)
{
    public string? BaseKey
    {
        get => ModDataAttribute.BaseKey;
        set => ModDataAttribute.BaseKey = value;
    }
    public SaveWhen SaveWhen => ModDataAttribute.SaveWhen;
    public LoadWhen LoadWhen => ModDataAttribute.LoadWhen;
    public SaveLocation SaveLocation => ModDataAttribute.SaveLocation;
}