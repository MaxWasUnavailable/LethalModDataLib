using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;
using LethalModDataLib.Features;
using LethalModDataLib.Helpers;

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
    protected virtual SaveLocation SaveLocation { get; set; } = SaveLocation.CurrentSave;

    /// <summary>
    ///     Gets an optional prefix suffix to add to the GetPrefix() method.
    /// </summary>
    protected virtual string OptionalPrefixSuffix { get; set; } = string.Empty;

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

        fields = fields.Where(field => !ModDataHelper.IsKBackingField(field)).ToArray();

        return fields.ToList();
    }

    /// <summary>
    ///     Gets all properties in the container.
    /// </summary>
    /// <returns> All properties in the container. </returns>
    private List<PropertyInfo> GetProperties()
    {
        var type = GetType();

        // Get all properties in the container
        var properties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                            BindingFlags.Static | BindingFlags.DeclaredOnly);

        // Ignore SaveLocation and OptionalPrefixSuffix properties, since those may be overridden in derived classes
        properties = properties.Where(property =>
            property.Name != nameof(SaveLocation) && property.Name != nameof(OptionalPrefixSuffix)).ToArray();

        return properties.ToList();
    }

    /// <summary>
    ///     Gets the prefix for moddata keys of fields in the container.
    /// </summary>
    /// <returns> The prefix for moddata keys of fields in the container. </returns>
    protected virtual string GetPrefix()
    {
        var type = GetType();
        var prefixSuffix = string.Empty;

        // if there is an optional prefix we add it to the prefix + "."
        if (!string.IsNullOrEmpty(OptionalPrefixSuffix))
            prefixSuffix = OptionalPrefixSuffix + ".";

        return type.Assembly.GetName().Name + "." + type.Name + "." + prefixSuffix;
    }

    /// <summary>
    ///     Saves all fields in the container.
    /// </summary>
    private void SaveFields()
    {
        var prefix = GetPrefix();

        foreach (var field in GetFields())
        {
            // If has IgnoreAttribute, check if it should be ignored
            var ignoreAttribute = field.GetCustomAttribute<ModDataIgnoreAttribute>();

            if (ignoreAttribute != null)
            {
                // If empty flags, always skip
                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.None))
                    continue;

                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.OnSave))
                    continue;

                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.IfNull))
                    if (field.GetValue(this) == null)
                        continue;

                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.IfDefault))
                {
                    var defaultValue = field.FieldType.IsValueType ? Activator.CreateInstance(field.FieldType) : null;

                    if (field.GetValue(this).Equals(defaultValue))
                        continue;
                }
            }

            var value = field.GetValue(this);

            SaveLoadHandler.SaveData(value, prefix + field.Name, SaveLocation, false);
        }
    }

    /// <summary>
    ///     Saves all properties in the container.
    /// </summary>
    private void SaveProperties()
    {
        var prefix = GetPrefix();

        foreach (var property in GetProperties())
        {
            // If property has no getter, skip it
            if (property.GetGetMethod(true) == null)
                continue;

            // If has IgnoreAttribute, check if it should be ignored
            var ignoreAttribute = property.GetCustomAttribute<ModDataIgnoreAttribute>();

            if (ignoreAttribute != null)
            {
                // If empty flags, always skip
                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.None))
                    continue;

                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.OnSave))
                    continue;

                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.IfNull))
                    if (property.GetValue(this) == null)
                        continue;

                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.IfDefault))
                {
                    var defaultValue = property.PropertyType.IsValueType
                        ? Activator.CreateInstance(property.PropertyType)
                        : null;

                    if (property.GetValue(this).Equals(defaultValue))
                        continue;
                }
            }

            var value = property.GetValue(this);

            SaveLoadHandler.SaveData(value, prefix + property.Name, SaveLocation, false);
        }
    }

    /// <summary>
    ///     Saves all fields in the container.
    /// </summary>
    public void Save()
    {
        PreSave();

        SaveFields();
        SaveProperties();

        PostSave();
    }

    /// <summary>
    ///     Called before saving.
    /// </summary>
    protected virtual void PreSave()
    {
    }

    /// <summary>
    ///     Called after saving.
    /// </summary>
    protected virtual void PostSave()
    {
    }

    /// <summary>
    ///     Loads all fields in the container.
    /// </summary>
    private void LoadFields()
    {
        var prefix = GetPrefix();

        foreach (var field in GetFields())
        {
            // If has IgnoreAttribute, check if it should be ignored
            var ignoreAttribute = field.GetCustomAttribute<ModDataIgnoreAttribute>();

            if (ignoreAttribute != null)
            {
                // If empty flags, always skip
                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.None))
                    continue;

                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.OnLoad))
                    continue;

                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.IfNull))
                    if (field.GetValue(this) == null)
                        continue;

                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.IfDefault))
                {
                    var defaultValue = field.FieldType.IsValueType ? Activator.CreateInstance(field.FieldType) : null;

                    if (field.GetValue(this).Equals(defaultValue))
                        continue;
                }
            }

            var value = SaveLoadHandler.LoadData<object>(prefix + field.Name, SaveLocation,
                autoAddGuid: false);

            field.SetValue(this, value);
        }
    }

    /// <summary>
    ///     Loads all properties in the container.
    /// </summary>
    private void LoadProperties()
    {
        var prefix = GetPrefix();

        foreach (var property in GetProperties())
        {
            // If property has no setter, skip it
            if (property.GetSetMethod(true) == null)
                continue;

            // If has IgnoreAttribute, check if it should be ignored
            var ignoreAttribute = property.GetCustomAttribute<ModDataIgnoreAttribute>();

            if (ignoreAttribute != null)
            {
                // If empty flags, always skip
                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.None))
                    continue;

                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.OnLoad))
                    continue;

                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.IfNull))
                    if (property.GetValue(this) == null)
                        continue;

                if (ignoreAttribute.IgnoreFlags.HasFlag(IgnoreFlags.IfDefault))
                {
                    var defaultValue = property.PropertyType.IsValueType
                        ? Activator.CreateInstance(property.PropertyType)
                        : null;

                    if (property.GetValue(this).Equals(defaultValue))
                        continue;
                }
            }

            var value = SaveLoadHandler.LoadData(prefix + property.Name, SaveLocation,
                autoAddGuid: false, defaultValue: property.GetValue(this));

            property.SetValue(this, value);
        }
    }

    /// <summary>
    ///     Loads all fields in the container.
    /// </summary>
    public void Load()
    {
        PreLoad();

        LoadFields();
        LoadProperties();

        PostLoad();
    }

    /// <summary>
    ///     Called before loading.
    /// </summary>
    protected virtual void PreLoad()
    {
    }

    /// <summary>
    ///     Called after loading.
    /// </summary>
    /// <remarks> This is a good place to sanity check loaded values. </remarks>
    protected virtual void PostLoad()
    {
    }
}