using System;
using LethalModDataLib.Enums;
using LethalModDataLib.Models;

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
    [Obsolete("Use the new ModDataConfiguration constructor instead.")]
    public ModDataAttribute(SaveWhen saveWhen, LoadWhen loadWhen, SaveLocation saveLocation, string? baseKey = null)
    {
        SaveWhen = saveWhen;
        LoadWhen = loadWhen;
        SaveLocation = saveLocation;
        BaseKey = baseKey;
        Configuration = new ModDataConfiguration(saveWhen, loadWhen, saveLocation, baseKey);
    }

    /// <summary>
    ///     Attribute to mark fields or properties to be saved and loaded by the mod data system.
    /// </summary>
    /// <param name="configuration"> Configuration for the mod data attribute. </param>
    public ModDataAttribute(ModDataConfiguration configuration)
    {
        SaveWhen = configuration.SaveWhen;
        LoadWhen = configuration.LoadWhen;
        SaveLocation = configuration.SaveLocation;
        BaseKey = configuration.BaseKey;
        Configuration = configuration;
    }

    /// <summary>
    ///     ModData Configuration for the attribute.
    /// </summary>
    public ModDataConfiguration Configuration { get; }

    /// <summary>
    ///     When to load the field.
    /// </summary>
    public LoadWhen LoadWhen { get; }

    /// <summary>
    ///     When to save the field.
    /// </summary>
    public SaveWhen SaveWhen { get; }

    /// <summary>
    ///     Where to save the field.
    /// </summary>
    public SaveLocation SaveLocation { get; }

    /// <summary>
    ///     Key prefix for the field. The ModData system will automatically set this to the mod's GUID unless it is set
    ///     manually.
    /// </summary>
    public string? BaseKey { get; set; }
}