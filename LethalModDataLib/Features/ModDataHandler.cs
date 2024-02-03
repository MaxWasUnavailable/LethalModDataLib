using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using LethalEventsLib.Events;
using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;

namespace LethalModDataLib.Features;

public static class ModDataHandler
{
    private static readonly Dictionary<FieldInfo, ModDataAttribute> ModDataEntries = new();
    private static readonly Dictionary<Assembly, string> PluginGuids = new();

    /// <summary>
    ///     Verifies that the key and file name are not null or empty.
    /// </summary>
    /// <param name="key"> Key to verify. </param>
    /// <param name="fileName"> File name to verify. </param>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    private static void VerifyKeyAndFileName(string key, string fileName)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty!");

        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("File name cannot be null or empty!");
    }

    /// <summary>
    ///     Gets the GUID of the plugin that called the method.
    /// </summary>
    /// <param name="assembly"> Assembly of the plugin that called the method. </param>
    /// <returns> The GUID of the plugin that called the method. </returns>
    private static string GetCallingPluginGuid(Assembly assembly)
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

    /// <summary>
    ///     Gets the moddata key for a field registered with the ModDataAttribute.
    /// </summary>
    /// <param name="field"> Field to get the moddata key for. </param>
    /// <returns> The moddata key for the field. </returns>
    public static string GetFieldKey(FieldInfo field)
    {
        return ModDataEntries[field].BaseKey + "." + field.Name;
    }

    /// <summary>
    ///     Generates a base key for a field registered with the ModDataAttribute.
    /// </summary>
    /// <param name="type"> Type of the field (used to fetch the namespace & class of the field's parent). </param>
    /// <param name="guid"> GUID of the plugin that registered the field. </param>
    /// <returns> The generated base key for the field. </returns>
    public static string GenerateFieldBaseKey(Type type, string guid)
    {
        return guid + "." + type.FullName;
    }

    /// <summary>
    ///     Deletes a moddata file.
    /// </summary>
    /// <param name="fileName"> Name of the file to delete. </param>
    private static void DeleteModDataFile(string fileName)
    {
        fileName += ".moddata";

        try
        {
            ES3.DeleteFile(fileName);
        }
        catch (Exception e)
        {
            LethalModDataLib.Logger?.LogError($"Failed to delete file {fileName}! Exception: {e}");
        }
    }

    #region Initialisation

    /// <summary>
    ///     Registers all mod data fields that are declared in assemblies of BepInEx plugins.
    /// </summary>
    private static void RegisterModDataAttributes()
    {
        foreach (var pluginInfo in Chainloader.PluginInfos.Values)
        foreach (var type in pluginInfo.Instance!.GetType().Assembly.GetTypes())
            AddModDataFields(pluginInfo.Metadata.GUID, type);
    }

    /// <summary>
    ///     Saves all mod data fields that are declared in BepInEx plugins.
    /// </summary>
    private static void AddModDataFields(string guid, Type type)
    {
        foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                             BindingFlags.Static))
            if (Attribute.IsDefined(field, typeof(ModDataAttribute)))
            {
                if (ModDataEntries.ContainsKey(field))
                {
                    LethalModDataLib.Logger?.LogWarning(
                        $"Field {field.Name} from {type.AssemblyQualifiedName} is already registered!");
                    continue;
                }

                ModDataEntries.Add(field, field.GetCustomAttribute<ModDataAttribute>());
                ModDataEntries[field].BaseKey ??= GenerateFieldBaseKey(type, guid);
                LethalModDataLib.Logger?.LogDebug(
                    $"Added field {field.Name} from {guid}.{type.FullName} to the mod data system!");
            }
    }

    /// <summary>
    ///     Initialises the mod data system.
    /// </summary>
    public static void Initialise()
    {
        LethalModDataLib.Logger?.LogInfo("Registering ModDataAttribute fields...");
        RegisterModDataAttributes();

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
    ///     Saves all mod data fields that have their SaveWhen set to OnSave.
    /// </summary>
    private static void OnSave(bool isChallengeFile, string saveFileName)
    {
        foreach (var field in ModDataEntries.Keys.Where(field => ModDataEntries[field].SaveWhen == SaveWhen.OnSave))
            if (!SaveData(field))
                LethalModDataLib.Logger?.LogWarning(
                    $"Failed to save field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName}!");
    }

    /// <summary>
    ///     Saves all mod data fields that have their SaveWhen set to OnAutoSave.
    /// </summary>
    private static void OnAutoSave(bool isChallengeFile, string saveFileName)
    {
        foreach (var field in ModDataEntries.Keys.Where(field =>
                     ModDataEntries[field].SaveWhen == SaveWhen.OnAutoSave))
            if (!SaveData(field))
                LethalModDataLib.Logger?.LogWarning(
                    $"Failed to save field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName}!");
    }

    /// <summary>
    ///     Loads all mod data fields that have their LoadWhen set to OnLoad.
    /// </summary>
    private static void OnLoad(bool isChallengeFile, string saveFileName)
    {
        foreach (var field in ModDataEntries.Keys.Where(field => ModDataEntries[field].LoadWhen == LoadWhen.OnLoad))
            if (!LoadData(field))
                LethalModDataLib.Logger?.LogWarning(
                    $"Failed to load field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName}!");
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

    #region LoadData

    /// <summary>
    ///     Loads data from a moddata file.
    /// </summary>
    /// <param name="key"> Key to load the data from. </param>
    /// <param name="fileName"> Name of the file to load the data from. </param>
    /// <param name="defaultValue"> Default value to return if the data could not be loaded. </param>
    /// <typeparam name="T"> Type of the data to load. </typeparam>
    /// <returns> The loaded data, or the default value if the data could not be loaded. </returns>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static T? LoadData<T>(string key, string fileName, T? defaultValue = default)
    {
        VerifyKeyAndFileName(key, fileName);

        fileName += ".moddata";

        try
        {
            LethalModDataLib.Logger?.LogDebug($"Loading data from file {fileName} with key {key}...");
            return ES3.KeyExists(key, fileName) ? ES3.Load<T>(key, fileName) : defaultValue;
        }
        catch (Exception e)
        {
            LethalModDataLib.Logger?.LogError(
                $"Failed to load data from file {fileName} with key {key}! Exception: {e}");
            return defaultValue;
        }
    }

    /// <summary>
    ///     Loads data from the moddata file matching the SaveLocation enum.
    /// </summary>
    /// <param name="key"> Key to load the data from. </param>
    /// <param name="saveLocation"> Save location enum to use for determining the file name. </param>
    /// <param name="defaultValue"> Default value to return if the data could not be loaded. </param>
    /// <param name="autoAddGuid"> Whether or not to automatically add the GUID of the plugin that called the method to the key. </param>
    /// <typeparam name="T"> Type of the data to load. </typeparam>
    /// <returns> The loaded data, or the default value if the data could not be loaded. </returns>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown if the save location is invalid. </exception>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static T? LoadData<T>(string key, T? defaultValue = default,
        SaveLocation saveLocation = SaveLocation.CurrentSave, bool autoAddGuid = true)
    {
        if (autoAddGuid)
        {
            var guid = GetCallingPluginGuid(Assembly.GetCallingAssembly());

            if (!key.StartsWith(guid))
                key = guid + "." + key;
        }

        return LoadData(key, saveLocation switch
        {
            SaveLocation.CurrentSave => ModDataHelper.GetCurrentSaveFileName(),
            SaveLocation.GeneralSave => ModDataHelper.GetGeneralSaveFileName(),
            _ => throw new ArgumentOutOfRangeException(nameof(saveLocation), saveLocation, "Invalid load location!")
        }, defaultValue);
    }

    /// <summary>
    ///     Loads data based on the ModDataAttribute attached to the field.
    /// </summary>
    /// <param name="field"> Field to load. </param>
    /// <returns> True if the data was loaded successfully, false otherwise. </returns>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static bool LoadData(FieldInfo field)
    {
        if (ModDataEntries[field].BaseKey == null)
        {
            LethalModDataLib.Logger?.LogWarning(
                $"Field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName} has no base key!");
            return false;
        }

        var key = GetFieldKey(field);
        var saveLocation = ModDataEntries[field].SaveLocation;

        var value = LoadData<object>(key, saveLocation: saveLocation, autoAddGuid: false);

        field.SetValue(null, value);
        return true;
    }

    #endregion

    #region SaveData

    /// <summary>
    ///     Saves data to a moddata file.
    /// </summary>
    /// <param name="data"> Data to save. </param>
    /// <param name="key"> Key to save the data under. </param>
    /// <param name="fileName"> Name of the file to save the data to. </param>
    /// <typeparam name="T"> Type of the data to save. </typeparam>
    /// <returns> True if the data was saved successfully, false otherwise. </returns>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static bool SaveData<T>(T? data, string key, string fileName)
    {
        VerifyKeyAndFileName(key, fileName);

        fileName += ".moddata";

        try
        {
            LethalModDataLib.Logger?.LogDebug($"Saving data to file {fileName} with key {key}...");
            ES3.Save(key, data, fileName);
            return true;
        }
        catch (Exception e)
        {
            LethalModDataLib.Logger?.LogError($"Failed to save data to file {fileName} with key {key}! Exception: {e}");
            return false;
        }
    }

    /// <summary>
    ///     Saves data to the moddata file matching the SaveLocation enum.
    /// </summary>
    /// <param name="data"> Data to save. </param>
    /// <param name="key"> Key to save the data under. </param>
    /// <param name="saveLocation"> Save location enum to use for determining the file name. </param>
    /// <param name="autoAddGuid">
    ///     Whether or not to automatically add the GUID of the plugin that called the method to the
    ///     key.
    /// </param>
    /// <typeparam name="T"> Type of the data to save. </typeparam>
    /// <returns> True if the data was saved successfully, false otherwise. </returns>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown if the save location is invalid. </exception>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static bool SaveData<T>(T? data, string key, SaveLocation saveLocation = SaveLocation.CurrentSave,
        bool autoAddGuid = true)
    {
        if (autoAddGuid)
        {
            var guid = GetCallingPluginGuid(Assembly.GetCallingAssembly());

            if (!key.StartsWith(guid))
                key = guid + "." + key;
        }

        return SaveData(data, key, saveLocation switch
        {
            SaveLocation.CurrentSave => ModDataHelper.GetCurrentSaveFileName(),
            SaveLocation.GeneralSave => ModDataHelper.GetGeneralSaveFileName(),
            _ => throw new ArgumentOutOfRangeException(nameof(saveLocation), saveLocation, "Invalid save location!")
        });
    }

    /// <summary>
    ///     Saves data based on the ModDataAttribute attached to the field.
    /// </summary>
    /// <param name="field"> Field to save. </param>
    /// <returns> True if the data was saved successfully, false otherwise. </returns>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static bool SaveData(FieldInfo field)
    {
        if (!ModDataEntries.ContainsKey(field))
        {
            LethalModDataLib.Logger?.LogWarning(
                $"Field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName} is not registered!");
            return false;
        }

        if (ModDataEntries[field].BaseKey == null)
        {
            LethalModDataLib.Logger?.LogWarning(
                $"Field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName} has no base key!");
            return false;
        }

        var key = GetFieldKey(field);
        var saveLocation = ModDataEntries[field].SaveLocation;

        var value = field.GetValue(null);

        return SaveData(value, key, saveLocation, false);
    }

    #endregion
}