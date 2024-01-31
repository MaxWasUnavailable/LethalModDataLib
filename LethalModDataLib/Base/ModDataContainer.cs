using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LethalModDataLib.Enums;
using LethalModDataLib.Features;

namespace LethalModDataLib.Base;

/// <summary>
///     Base class for ModData containers.
/// </summary>
public abstract class ModDataContainer
{
    /// <summary>
    ///     Gets the save location for the container.
    /// </summary>
    /// <remarks> Edit this to change the save location. </remarks>
    private SaveLocation SaveLocation { get; } = SaveLocation.CurrentSave;

    /// <summary>
    ///     Gets all fields in the container.
    /// </summary>
    /// <returns> All fields in the container. </returns>
    private List<FieldInfo> GetFields()
    {
        var type = GetType();

        // Get all fields in the container
        var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                    BindingFlags.Static);

        return fields.ToList();
    }

    /// <summary>
    ///     Gets the prefix for moddata keys of fields in the container.
    /// </summary>
    /// <returns> The prefix for moddata keys of fields in the container. </returns>
    private string GetPrefix()
    {
        var type = GetType();

        return type.Assembly.GetName().Name + "." + type.Name + ".";
    }

    /// <summary>
    ///     Saves all fields in the container.
    /// </summary>
    public void Save()
    {
        PreSave();

        var prefix = GetPrefix();

        foreach (var field in GetFields())
        {
            var value = field.GetValue(this);

            ModDataHandler.SaveData(value, prefix + field.Name, SaveLocation, autoAddGuid: false);
        }

        PostSave();
    }

    /// <summary>
    ///     Called before saving.
    /// </summary>
    internal virtual void PreSave()
    {
    }

    /// <summary>
    ///     Called after saving.
    /// </summary>
    internal virtual void PostSave()
    {
    }

    /// <summary>
    ///     Loads all fields in the container.
    /// </summary>
    public void Load()
    {
        PreLoad();

        var prefix = GetPrefix();

        foreach (var field in GetFields())
        {
            var value = ModDataHandler.LoadData<object>(prefix + field.Name, saveLocation: SaveLocation, autoAddGuid: false);

            field.SetValue(this, value);
        }

        PostLoad();
    }

    /// <summary>
    ///     Called before loading.
    /// </summary>
    internal virtual void PreLoad()
    {
    }

    /// <summary>
    ///     Called after loading.
    /// </summary>
    /// <remarks> This is a good place to sanity check loaded values. </remarks>
    internal virtual void PostLoad()
    {
    }
}