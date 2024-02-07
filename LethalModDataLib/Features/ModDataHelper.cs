using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;

namespace LethalModDataLib.Features;

public static class ModDataHelper
{
    private static readonly Dictionary<Assembly, string> PluginGuids = new();

    /// <summary>
    ///     Gets the field info for a field. Used to manually handle saving and/or loading of an attributed field.
    /// </summary>
    /// <param name="obj"> Object to get the field info from. </param>
    /// <param name="fieldName"> Name of the field to get the field info for. </param>
    /// <returns> The field info for the field. </returns>
    /// <remarks> It is recommended to use nameof() to get the field name. </remarks>
    public static FieldInfo? GetFieldInfo(object obj, string fieldName)
    {
        return obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                                 BindingFlags.Static);
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