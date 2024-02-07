using System;
using LethalModDataLib.Enums;

namespace LethalModDataLib.Attributes;

/// <summary>
///     Attribute to mark static fields or properties to be saved and loaded by the mod data system.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ModDataAttribute : Attribute
{
    /// <summary>
    ///     Attribute to mark fields or properties to be saved and loaded by the mod data system.
    /// </summary>
    /// <param name="saveWhen"> When to save the field. <see cref="SaveWhen" /> </param>
    /// <param name="loadWhen"> When to load the field. <see cref="LoadWhen" /> </param>
    /// <param name="saveLocation"> Where to save the field. <see cref="SaveLocation" /> </param>
    /// <param name="baseKey">
    ///     Key prefix for the field. The ModData system will automatically set this to the mod's GUID
    ///     unless it is set manually.
    /// </param>
    public ModDataAttribute(SaveWhen saveWhen, LoadWhen loadWhen, SaveLocation saveLocation, string? baseKey = null)
    {
        SaveWhen = saveWhen;
        LoadWhen = loadWhen;
        SaveLocation = saveLocation;
        BaseKey = baseKey;
    }

    public LoadWhen LoadWhen { get; }
    public SaveWhen SaveWhen { get; }
    public SaveLocation SaveLocation { get; }

    /// <summary>
    ///     Key prefix for the field. The ModData system will automatically set this to the mod's GUID unless it is set
    ///     manually.
    /// </summary>
    public string? BaseKey { get; set; }
}