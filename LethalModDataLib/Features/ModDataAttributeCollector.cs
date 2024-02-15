using System;
using System.Reflection;
using BepInEx.Bootstrap;
using LethalModDataLib.Attributes;
using LethalModDataLib.Models;

namespace LethalModDataLib.Features;

/// <summary>
///     Handles the automated collection and registration of static fields and properties decorated with ModData attributes
///     in BepInEx plugins.
/// </summary>
public static class ModDataAttributeCollector
{
    /// <summary>
    ///     Registers all static fields & properties decorated with ModData attributes in assemblies of BepInEx plugins.
    /// </summary>
    internal static void RegisterModDataAttributes()
    {
        foreach (var pluginInfo in Chainloader.PluginInfos.Values)
        foreach (var type in pluginInfo.Instance!.GetType().Assembly.GetTypes())
            RegisterModDataAttributes(pluginInfo.Metadata.GUID, type);
    }

    /// <summary>
    ///     Registers all fields & properties decorated with ModData attributes in the given type.
    /// </summary>
    /// <param name="guid"> GUID of the plugin that registered the fields. </param>
    /// <param name="type"> Type to register the fields from. </param>
    /// <param name="instance"> Instance of the class to register the fields from. </param>
    /// <param name="keySuffix"> Suffix to append to the key. </param>
    internal static void RegisterModDataAttributes(string guid, Type type, object? instance = null,
        string? keySuffix = null)
    {
        AddModDataFields(guid, type, instance, keySuffix);
        AddModDataProperties(guid, type, instance, keySuffix);
    }

    /// <summary>
    ///     Include static if instance is null, else instance
    /// </summary>
    /// <param name="instance"> Instance of the class to register. </param>
    /// <returns> The binding flags for the instance. </returns>
    private static BindingFlags GetBindingFlags(object? instance = null)
    {
        return instance == null
            ? BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
            : BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    }

    /// <summary>
    ///     Registers all fields decorated with ModData attributes in the given type.
    /// </summary>
    /// <param name="guid"> GUID of the plugin that registered the fields. </param>
    /// <param name="type"> Type to register the fields from. </param>
    /// <param name="instance"> Instance of the class to register the fields from. </param>
    /// <param name="keySuffix"> Suffix to append to the key. </param>
    private static void AddModDataFields(string guid, Type type, object? instance = null, string? keySuffix = null)
    {
        foreach (var field in type.GetFields(GetBindingFlags(instance)))
            if (Attribute.IsDefined(field, typeof(ModDataAttribute)))
            {
                var fieldKey = new FieldKey(field, instance);
                ModDataHandler.AddModData(guid, type, fieldKey, keySuffix);
            }
    }

    /// <summary>
    ///     Registers all properties decorated with ModData attributes in the given type.
    /// </summary>
    /// <param name="guid"> GUID of the plugin that registered the properties. </param>
    /// <param name="type"> Type to register the properties from. </param>
    /// <param name="instance"> Instance of the class to register the properties from. </param>
    /// <param name="keySuffix"> Suffix to append to the key. </param>
    private static void AddModDataProperties(string guid, Type type, object? instance = null, string? keySuffix = null)
    {
        foreach (var property in type.GetProperties(GetBindingFlags(instance)))
            if (Attribute.IsDefined(property, typeof(ModDataAttribute)))
            {
                var propertyKey = new PropertyKey(property, instance);
                ModDataHandler.AddModData(guid, type, propertyKey, keySuffix);
            }
    }
}