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
    /// <param name="configuration"> Configuration for the mod data attribute. </param>
    public ModDataAttribute(ModDataConfiguration configuration)
    {
        Configuration = configuration;
        BaseKey = configuration.BaseKey;
    }

    /// <summary>
    ///     ModData Configuration for the attribute.
    /// </summary>
    private ModDataConfiguration Configuration { get; }

    /// <summary>
    ///     When to load the field.
    /// </summary>
    public LoadWhen LoadWhen => Configuration.LoadWhen;

    /// <summary>
    ///     When to save the field.
    /// </summary>
    public SaveWhen SaveWhen => Configuration.SaveWhen;

    /// <summary>
    ///     Where to save the field.
    /// </summary>
    public SaveLocation SaveLocation => Configuration.SaveLocation;

    /// <summary>
    ///     Key prefix for the field. The ModData system will automatically set this to the mod's GUID unless it is set
    ///     manually.
    /// </summary>
    public string? BaseKey { get; set; }
}