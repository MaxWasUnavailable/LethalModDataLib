using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using LethalModDataLib.Enums;
using LethalModDataLib.Events;
using LethalModDataLib.Helpers;
using LethalModDataLib.Interfaces;
using LethalModDataLib.Models;

namespace LethalModDataLib.Features;

/// <summary>
///     General handler for the ModDataAttribute system.
/// </summary>
public static class ModDataHandler
{
    internal static Dictionary<IModDataKey, ModDataValue> ModDataValues { get; } = new();

    /// <summary>
    ///     For a field or property registered with the ModDataAttribute, gets an ES3 key string for it.
    /// </summary>
    /// <param name="iModDataKey"> IModDataKey object to get the ES3 key string for. </param>
    /// <returns> ES3 key string for the field or property. </returns>
    internal static string ToES3KeyString(this IModDataKey iModDataKey)
    {
        if (!ModDataValues.TryGetValue(iModDataKey, out var modDataValue))
            throw new ArgumentException(
                $"Field or property with name {iModDataKey.Name} from {iModDataKey.AssemblyQualifiedName} is not registered with the ModDataAttribute!");

        if (modDataValue.BaseKey == null)
            throw new ArgumentException(
                $"Field or property with name {iModDataKey.Name} from {iModDataKey.AssemblyQualifiedName} has no base key!");

        var key = modDataValue.BaseKey + ".";

        if (!string.IsNullOrEmpty(modDataValue.KeySuffix))
            return key + modDataValue.KeySuffix + "." + iModDataKey.Name;

        // Else
        return key + iModDataKey.Name;
    }

    /// <summary>
    ///     Registers an instance of a class that has fields or properties registered with the ModDataAttribute.
    /// </summary>
    /// <param name="instance"> Instance of the class to register. </param>
    /// <param name="keySuffix">
    ///     Suffix to append to the key. Strongly recommended to use if multiple instances of the same class will
    ///     be registered.
    /// </param>
    /// <remarks> This needs to be called on any class that uses the ModDataAttribute on non-static fields or properties. </remarks>
    public static void RegisterInstance(object instance, string? keySuffix = null)
    {
        LethalModDataLib.Logger?.LogDebug($"Registering instance {instance.GetType().FullName}...");
        var guid = ModDataHelper.GetCallingPluginGuid(Assembly.GetCallingAssembly());

        ModDataAttributeCollector.RegisterModDataAttributes(guid, instance.GetType(), instance, keySuffix);
    }

    /// <summary>
    ///     De-registers an instance of a class that has fields or properties registered with the ModDataAttribute.
    /// </summary>
    /// <param name="instance"> Instance of the class to de-register. </param>
    public static void DeRegisterInstance(object instance)
    {
        LethalModDataLib.Logger?.LogDebug($"De-registering instance {instance.GetType().FullName}...");
        ModDataAttributeCollector.DeRegisterModDataAttributes(instance);
    }

    /// <summary>
    ///     Adds a field or property to the mod data system.
    /// </summary>
    /// <param name="guid"> GUID of the plugin that registered the field or property. </param>
    /// <param name="type"> Type of the field or property. </param>
    /// <param name="modDataKey"> ModDataKey of the field or property. </param>
    /// <param name="keySuffix"> Key suffix to save in the ModDataValue. </param>
    internal static void AddModData(string guid, Type type, IModDataKey modDataKey, string? keySuffix = null)
    {
        if (ModDataValues.ContainsKey(modDataKey))
        {
            LethalModDataLib.Logger?.LogWarning(
                $"Field or property with name {modDataKey.Name} from {type.AssemblyQualifiedName} is already registered!");
            return;
        }

        modDataKey.TryGetValue(out var originalValue);

        ModDataValues.Add(modDataKey, new ModDataValue(modDataKey.GetModDataAttribute(), keySuffix, originalValue));
        ModDataValues[modDataKey].BaseKey ??= ModDataHelper.GenerateBaseKey(type, guid);
        LethalModDataLib.Logger?.LogDebug(
            $"Added field or property with name {modDataKey.Name} from {type.AssemblyQualifiedName} to the mod data system!");

        if (ModDataValues[modDataKey].LoadWhen.HasFlag(LoadWhen.OnRegister))
            HandleLoadModData(modDataKey);
    }

    #region Initialisation

    /// <summary>
    ///     Initialises the mod data system.
    /// </summary>
    internal static void Initialise()
    {
        LethalModDataLib.Logger?.LogInfo("Registering ModDataAttribute fields...");

        var timer = new Stopwatch();
        timer.Start();

        ModDataAttributeCollector.RegisterModDataAttributes();

        timer.Stop();
        LethalModDataLib.Logger?.LogInfo($"ModDataAttribute registration took {timer.ElapsedMilliseconds}ms.");

        LethalModDataLib.Logger?.LogInfo("Hooking up save, load and delete events...");
        SaveLoadEvents.PostSaveGameEvent += OnSave;
        SaveLoadEvents.PostAutoSaveEvent += OnAutoSave;
        SaveLoadEvents.PostLoadGameEvent += OnLoad;
        SaveLoadEvents.PostDeleteSaveEvent += OnDeleteSave;
        SaveLoadEvents.PostResetSavedGameValuesEvent += OnGameOver;

        LethalModDataLib.Logger?.LogInfo("ModDataHandler initialised!");
    }

    #endregion

    #region EventHandlers

    /// <summary>
    ///     Saves the mod data attributed object.
    /// </summary>
    /// <param name="modDataKey"> ModDataKey of the object to save. </param>
    internal static void HandleSaveModData(this IModDataKey modDataKey)
    {
        if (!SaveLoadHandler.SaveData(modDataKey))
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to save field or property {modDataKey.Name} from {modDataKey.AssemblyQualifiedName}!");
    }

    /// <summary>
    ///     Loads the mod data attributed object.
    /// </summary>
    /// <param name="modDataKey"> ModDataKey of the object to load. </param>
    internal static void HandleLoadModData(this IModDataKey modDataKey)
    {
        if (!SaveLoadHandler.LoadData(modDataKey))
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to load field or property {modDataKey.Name} from {modDataKey.AssemblyQualifiedName}!");
    }

    /// <summary>
    ///     Deletes a moddata file based on the save file name.
    /// </summary>
    /// <param name="saveName"> Name of the save file to delete the moddata file for. </param>
    private static void DeleteModDataFile(string saveName)
    {
        saveName += ".moddata";

        try
        {
            ES3.DeleteFile(saveName);
        }
        catch (Exception e)
        {
            LethalModDataLib.Logger?.LogError($"Failed to delete file {saveName}! Exception: {e}");
        }
    }

    private static void ResetModData(this IModDataKey modDataKey)
    {
        if (!modDataKey.TrySetValue(ModDataValues[modDataKey].OriginalValue))
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to reset field or property {modDataKey.Name} from {modDataKey.AssemblyQualifiedName}!");
    }

    /// <summary>
    ///     Saves all mod data attributed objects that have their SaveWhen set to OnSave.
    /// </summary>
    private static void OnSave(bool isChallengeFile, string saveFileName)
    {
        foreach (var modDataKey in ModDataValues.Keys.Where(modDataKey =>
                     ModDataValues[modDataKey].SaveWhen.HasFlag(SaveWhen.OnSave)))
            modDataKey.HandleSaveModData();
    }

    /// <summary>
    ///     Saves all mod data attributed objects that have their SaveWhen set to OnAutoSave.
    /// </summary>
    private static void OnAutoSave(bool isChallengeFile, string saveFileName)
    {
        foreach (var modDataKey in ModDataValues.Keys.Where(modDataKey =>
                     ModDataValues[modDataKey].SaveWhen.HasFlag(SaveWhen.OnAutoSave)))
            modDataKey.HandleSaveModData();
    }

    /// <summary>
    ///     Loads all mod data attributed objects that have their LoadWhen set to OnLoad.
    /// </summary>
    private static void OnLoad(bool isChallengeFile, string saveFileName)
    {
        foreach (var modDataKey in ModDataValues.Keys.Where(modDataKey =>
                     ModDataValues[modDataKey].LoadWhen.HasFlag(LoadWhen.OnLoad)))
            modDataKey.HandleLoadModData();
    }

    /// <summary>
    ///     Deletes the mod data file matching the save file name.
    /// </summary>
    /// <param name="filePath"> Save file path to delete the mod data file for. </param>
    private static void OnDeleteSave(string filePath)
    {
        DeleteModDataFile(filePath);
    }

    /// <summary>
    ///     Resets all mod data attributed objects that have their ResetWhen set to OnGameOver.
    /// </summary>
    private static void OnGameOver()
    {
        foreach (var modDataKey in ModDataValues.Keys.Where(modDataKey =>
                     ModDataValues[modDataKey].ResetWhen.HasFlag(ResetWhen.OnGameOver)))
            modDataKey.ResetModData();
    }

    #endregion
}