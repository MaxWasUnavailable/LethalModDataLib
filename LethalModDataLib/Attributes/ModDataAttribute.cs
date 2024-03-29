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
    /// <param name="resetWhen"> When to reset the field. <see cref="ResetWhen" /> </param>
    /// <param name="baseKey">
    ///     Key prefix for the field. The ModData system will automatically set this to the mod's GUID
    ///     unless it is set manually.
    /// </param>
    public ModDataAttribute(SaveWhen saveWhen = SaveWhen.OnSave, LoadWhen loadWhen = LoadWhen.OnLoad,
        SaveLocation saveLocation = SaveLocation.CurrentSave, ResetWhen resetWhen = ResetWhen.Manual,
        string? baseKey = null)
    {
        SaveWhen = saveWhen;
        LoadWhen = loadWhen;
        SaveLocation = saveLocation;
        ResetWhen = resetWhen;
        BaseKey = baseKey;
    }

    /// <summary>
    ///     Attribute to mark fields or properties to be saved and loaded by the mod data system.
    ///     Empty constructor to use default values, or set them through property initialization.
    /// </summary>
    public ModDataAttribute()
    {
    }

    /// <summary>
    ///     When to load the field or property.
    /// </summary>
    public LoadWhen LoadWhen { get; set; } = LoadWhen.OnLoad;

    /// <summary>
    ///     When to save the field or property.
    /// </summary>
    public SaveWhen SaveWhen { get; set; } = SaveWhen.OnSave;

    /// <summary>
    ///     When to reset the field or property.
    /// </summary>
    public ResetWhen ResetWhen { get; set; } = ResetWhen.Manual;

    /// <summary>
    ///     Where to save the field or property.
    /// </summary>
    public SaveLocation SaveLocation { get; set; } = SaveLocation.CurrentSave;

    /// <summary>
    ///     Key prefix for the field or property. The ModData system will automatically set this to the mod's GUID unless it is
    ///     set manually.
    /// </summary>
    public string? BaseKey { get; set; }
}