using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LethalEventsLib.Events;
using LethalModDataLib.Enums;
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
    ///     Gets the moddata key for a field or property registered with the ModDataAttribute.
    /// </summary>
    /// <param name="iModDataKey"> IModDataKey object to get the moddata key for. </param>
    /// <returns> The moddata key for the field. </returns>
    internal static string GetModDataKey(IModDataKey iModDataKey)
    {
        if (!ModDataValues.TryGetValue(iModDataKey, out var modDataValue))
            throw new ArgumentException($"Field {iModDataKey.Name} is not registered with the ModDataAttribute!");

        if (modDataValue.BaseKey == null)
            throw new ArgumentException(
                $"Field {iModDataKey.Name} from {iModDataKey.AssemblyQualifiedName} has no base key!");

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
                $"Property {modDataKey.Name} from {type.AssemblyQualifiedName} is already registered!");
            return;
        }

        ModDataValues.Add(modDataKey, new ModDataValue(modDataKey.GetModDataAttribute(), keySuffix));
        ModDataValues[modDataKey].BaseKey ??= ModDataHelper.GenerateBaseKey(type, guid);
        LethalModDataLib.Logger?.LogDebug(
            $"Added property {modDataKey.Name} from {guid}.{type.FullName} to the mod data system!");
    }

    #region Initialisation

    /// <summary>
    ///     Initialises the mod data system.
    /// </summary>
    internal static void Initialise()
    {
        LethalModDataLib.Logger?.LogInfo("Registering ModDataAttribute fields...");
        ModDataAttributeCollector.RegisterModDataAttributes();

        LethalModDataLib.Logger?.LogInfo("Hooking up save, load and delete events...");
        SystemEvents.PostSaveGameEvent += OnSave;
        SystemEvents.PostAutoSaveShipDataEvent += OnAutoSave;
        SystemEvents.PostLoadGameEvent += OnLoad;
        SystemEvents.PostDeleteFileEvent += OnDeleteSave;

        LethalModDataLib.Logger?.LogInfo("ModDataHandler initialised!");
    }

    #endregion

    #region EventHandlers

    /// <summary>
    ///     Saves the mod data attributed object.
    /// </summary>
    /// <param name="modDataKey"> ModDataKey of the object to save. </param>
    private static void HandleSaveModData(IModDataKey modDataKey)
    {
        if (!SaveLoadHandler.SaveData(modDataKey))
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to save field {modDataKey.Name} from {modDataKey.AssemblyQualifiedName}!");
    }

    /// <summary>
    ///     Loads the mod data attributed object.
    /// </summary>
    /// <param name="modDataKey"> ModDataKey of the object to load. </param>
    private static void HandleLoadModData(IModDataKey modDataKey)
    {
        if (!SaveLoadHandler.LoadData(modDataKey))
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to load field {modDataKey.Name} from {modDataKey.AssemblyQualifiedName}!");
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

    /// <summary>
    ///     Saves all mod data attributed objects that have their SaveWhen set to OnSave.
    /// </summary>
    private static void OnSave(bool isChallengeFile, string saveFileName)
    {
        foreach (var modDataKey in ModDataValues.Keys.Where(modDataKey =>
                     ModDataValues[modDataKey].SaveWhen == SaveWhen.OnSave))
            HandleSaveModData(modDataKey);
    }

    /// <summary>
    ///     Saves all mod data attributed objects that have their SaveWhen set to OnAutoSave.
    /// </summary>
    private static void OnAutoSave(bool isChallengeFile, string saveFileName)
    {
        foreach (var modDataKey in ModDataValues.Keys.Where(modDataKey =>
                     ModDataValues[modDataKey].SaveWhen == SaveWhen.OnAutoSave))
            HandleSaveModData(modDataKey);
    }

    /// <summary>
    ///     Loads all mod data attributed objects that have their LoadWhen set to OnLoad.
    /// </summary>
    private static void OnLoad(bool isChallengeFile, string saveFileName)
    {
        foreach (var modDataKey in ModDataValues.Keys.Where(modDataKey =>
                     ModDataValues[modDataKey].LoadWhen == LoadWhen.OnLoad))
            HandleLoadModData(modDataKey);
    }

    /// <summary>
    ///     Deletes the mod data file matching the save file name.
    /// </summary>
    /// <param name="saveFileNum"> Save file number. </param>
    private static void OnDeleteSave(int saveFileNum)
    {
        var saveFileName = saveFileNum switch
        {
            -1 => GameNetworkManager.LCchallengeFileName,
            0 => GameNetworkManager.LCsaveFile1Name,
            1 => GameNetworkManager.LCsaveFile2Name,
            2 => GameNetworkManager.LCsaveFile3Name,
            _ => GameNetworkManager.LCsaveFile1Name
        };

        DeleteModDataFile(saveFileName);
    }

    #endregion
}