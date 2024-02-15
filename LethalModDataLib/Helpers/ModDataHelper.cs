using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using LethalModDataLib.Interfaces;
using LethalModDataLib.Models;

namespace LethalModDataLib.Features;

public static class ModDataHelper
{
    private static readonly Dictionary<Assembly, string> PluginGuids = new();

    /// <summary>
    ///     Gets an IModDataKey object for a field or property.
    ///     Used to manually handle saving and/or loading of an attributed field or property.
    ///     If the object isn't an instance, the field or property must be static.
    /// </summary>
    /// <param name="obj"> Object to get the field info from. </param>
    /// <param name="fieldName"> Name of the field to get the field info for. </param>
    /// <returns> The field info for the field. </returns>
    /// <remarks> It is recommended to use nameof() to get the field name. </remarks>
    public static IModDataKey? GetIModDataKey(object obj, string fieldName)
    {
        // Check if object is an instance
        object? instance = null;
        if (obj.GetType().IsInstanceOfType(obj))
            instance = obj;

        // Check if field with the given name exists
        var fieldInfo = obj.GetType()
            .GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo != null)
            return new FieldKey(fieldInfo, instance);

        // Else, check if property with the given name exists
        var propertyInfo = obj.GetType()
            .GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (propertyInfo != null)
            return new PropertyKey(propertyInfo, instance);

        throw new ArgumentException($"Field or property {fieldName} does not exist in {obj.GetType().FullName}!");
    }

    /// <summary>
    ///     Gets the current save file name.
    /// </summary>
    /// <returns> The current save file name. </returns>
    /// <remarks> When used too early, this will return the last save file used by the player. </remarks>
    public static string GetCurrentSaveFileName()
    {
        return GameNetworkManager.Instance.currentSaveFileName;
    }

    /// <summary>
    ///     Gets the general save file name.
    /// </summary>
    /// <returns> The general save file name. </returns>
    public static string GetGeneralSaveFileName()
    {
        return GameNetworkManager.generalSaveDataName;
    }

    /// <summary>
    ///     Checks if a field is a k__BackingField.
    /// </summary>
    /// <param name="fieldInfo"> Field info to check. </param>
    /// <returns> True if the field is a k__BackingField, false otherwise. </returns>
    /// <remarks>
    ///     This is technically an imperfect check, since a user *could* name their fields with k__BackingField. Considering
    ///     as good as no one would do that, this is a good enough check.
    /// </remarks>
    public static bool IsKBackingField(FieldInfo fieldInfo)
    {
        return fieldInfo.Name.Contains("k__BackingField");
    }

    /// <summary>
    ///     Generates a base key for a field or property registered with the ModDataAttribute.
    /// </summary>
    /// <param name="type"> Type of the field or property (used to fetch the namespace & class of the its parent). </param>
    /// <param name="guid"> GUID of the plugin that registered the field or property. </param>
    /// <returns> The generated base key for the field or property. </returns>
    public static string GenerateBaseKey(Type type, string guid)
    {
        return guid + "." + type.FullName;
    }

    /// <summary>
    ///     Gets the GUID of the plugin that called the method.
    /// </summary>
    /// <param name="assembly"> Assembly of the plugin that called the method. </param>
    /// <returns> The GUID of the plugin that called the method. </returns>
    public static string GetCallingPluginGuid(Assembly assembly)
    {
        if (PluginGuids.TryGetValue(assembly, out var guid))
            return guid;

        var callerPluginInfo = Chainloader.PluginInfos.Values.FirstOrDefault(pluginInfo =>
            pluginInfo.Instance?.GetType().Assembly == assembly);

        if (callerPluginInfo == null)
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to get plugin info for assembly {assembly.FullName}!");

        PluginGuids.Add(assembly, callerPluginInfo?.Metadata?.GUID ?? "Unknown");
        return PluginGuids[assembly];
    }
}