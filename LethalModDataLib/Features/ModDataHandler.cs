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
    private static readonly Dictionary<FieldInfo, ModDataAttribute> ModDataFields = new();
    private static readonly Dictionary<PropertyInfo, ModDataAttribute> ModDataProperties = new();

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
    ///     Gets the moddata key for a field registered with the ModDataAttribute.
    /// </summary>
    /// <param name="field"> Field to get the moddata key for. </param>
    /// <param name="keyPostfix"> Postfix to append to the key. Used for instance-specific keys. </param>
    /// <returns> The moddata key for the field. </returns>
    private static string GetFieldKey(FieldInfo field, string? keyPostfix = null)
    {
        var key = ModDataFields[field].BaseKey + ".";

        if (keyPostfix != null)
            return key + keyPostfix + "." + field.Name;

        return key + field.Name;
    }

    /// <summary>
    ///     Generates a base key for a property registered with the ModDataAttribute.
    /// </summary>
    /// <param name="property"> Property to get the moddata key for. </param>
    /// <param name="keyPostfix"> Postfix to append to the key. Used for instance-specific keys. </param>
    /// <returns> The moddata key for the property. </returns>
    private static string GetPropertyKey(PropertyInfo property, string? keyPostfix = null)
    {
        var key = ModDataProperties[property].BaseKey + ".";

        if (keyPostfix != null)
            return key + keyPostfix + "." + property.Name;

        return key + property.Name;
    }

    /// <summary>
    ///     Generates a base key for a field or property registered with the ModDataAttribute.
    /// </summary>
    /// <param name="type"> Type of the field or property (used to fetch the namespace & class of the its parent). </param>
    /// <param name="guid"> GUID of the plugin that registered the field or property. </param>
    /// <returns> The generated base key for the field or property. </returns>
    private static string GenerateBaseKey(Type type, string guid)
    {
        return guid + "." + type.FullName;
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

    #region Initialisation

    /// <summary>
    ///     Registers all fields & properties decorated with ModData attributes in assemblies of BepInEx plugins.
    /// </summary>
    private static void RegisterModDataAttributes()
    {
        foreach (var pluginInfo in Chainloader.PluginInfos.Values)
        foreach (var type in pluginInfo.Instance!.GetType().Assembly.GetTypes())
        {
            AddModDataFields(pluginInfo.Metadata.GUID, type);
            AddModDataProperties(pluginInfo.Metadata.GUID, type);
        }
    }

    /// <summary>
    ///     Registers all fields decorated with ModData attributes in the given type.
    /// </summary>
    /// <param name="guid"> GUID of the plugin that registered the fields. </param>
    /// <param name="type"> Type to register the fields from. </param>
    private static void AddModDataFields(string guid, Type type)
    {
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            if (Attribute.IsDefined(field, typeof(ModDataAttribute)))
            {
                if (!field.IsStatic)
                {
                    LethalModDataLib.Logger?.LogWarning(
                        $"Field {field.Name} from {type.AssemblyQualifiedName} is not static! " +
                        "ModData attributed fields must be static!");
                    continue;
                }

                if (ModDataFields.ContainsKey(field))
                {
                    LethalModDataLib.Logger?.LogWarning(
                        $"Field {field.Name} from {type.AssemblyQualifiedName} is already registered!");
                    continue;
                }

                ModDataFields.Add(field, field.GetCustomAttribute<ModDataAttribute>());
                ModDataFields[field].BaseKey ??= GenerateBaseKey(type, guid);
                LethalModDataLib.Logger?.LogDebug(
                    $"Added field {field.Name} from {guid}.{type.FullName} to the mod data system!");
            }
    }

    private static void AddModDataProperties(string guid, Type type)
    {
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            if (Attribute.IsDefined(property, typeof(ModDataAttribute)))
            {
                if (ModDataProperties.ContainsKey(property))
                {
                    LethalModDataLib.Logger?.LogWarning(
                        $"Property {property.Name} from {type.AssemblyQualifiedName} is already registered!");
                    continue;
                }

                ModDataProperties.Add(property, property.GetCustomAttribute<ModDataAttribute>());
                ModDataProperties[property].BaseKey ??= GenerateBaseKey(type, guid);
                LethalModDataLib.Logger?.LogDebug(
                    $"Added property {property.Name} from {guid}.{type.FullName} to the mod data system!");
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

    private static void HandleSaveField(FieldInfo field)
    {
        if (!SaveData(field))
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to save field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName}!");
    }

    private static void HandleSaveProperty(PropertyInfo property)
    {
        if (!SaveData(property))
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to save property {property.Name} from {property.DeclaringType?.AssemblyQualifiedName}!");
    }

    private static void HandleLoadField(FieldInfo field)
    {
        if (!LoadData(field))
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to load field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName}!");
    }

    private static void HandleLoadProperty(PropertyInfo property)
    {
        if (!LoadData(property))
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to load property {property.Name} from {property.DeclaringType?.AssemblyQualifiedName}!");
    }

    /// <summary>
    ///     Saves all mod data fields that have their SaveWhen set to OnSave.
    /// </summary>
    private static void OnSave(bool isChallengeFile, string saveFileName)
    {
        foreach (var field in ModDataFields.Keys.Where(field => ModDataFields[field].SaveWhen == SaveWhen.OnSave))
            HandleSaveField(field);

        foreach (var property in ModDataProperties.Keys.Where(property =>
                     ModDataProperties[property].SaveWhen == SaveWhen.OnSave))
            HandleSaveProperty(property);
    }

    /// <summary>
    ///     Saves all mod data fields that have their SaveWhen set to OnAutoSave.
    /// </summary>
    private static void OnAutoSave(bool isChallengeFile, string saveFileName)
    {
        foreach (var field in ModDataFields.Keys.Where(field => ModDataFields[field].SaveWhen == SaveWhen.OnAutoSave))
            HandleSaveField(field);

        foreach (var property in ModDataProperties.Keys.Where(property =>
                     ModDataProperties[property].SaveWhen == SaveWhen.OnAutoSave))
            HandleSaveProperty(property);
    }

    /// <summary>
    ///     Loads all mod data fields that have their LoadWhen set to OnLoad.
    /// </summary>
    private static void OnLoad(bool isChallengeFile, string saveFileName)
    {
        foreach (var field in ModDataFields.Keys.Where(field => ModDataFields[field].LoadWhen == LoadWhen.OnLoad))
            HandleLoadField(field);

        foreach (var property in ModDataProperties.Keys.Where(property =>
                     ModDataProperties[property].LoadWhen == LoadWhen.OnLoad))
            HandleLoadProperty(property);
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
    /// <param name="autoAddGuid">
    ///     Whether or not to automatically add the GUID of the plugin that called the method to the
    ///     key.
    /// </param>
    /// <typeparam name="T"> Type of the data to load. </typeparam>
    /// <returns> The loaded data, or the default value if the data could not be loaded. </returns>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown if the save location is invalid. </exception>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static T? LoadData<T>(string key, T? defaultValue = default,
        SaveLocation saveLocation = SaveLocation.CurrentSave, bool autoAddGuid = true)
    {
        if (autoAddGuid)
        {
            var guid = ModDataHelper.GetCallingPluginGuid(Assembly.GetCallingAssembly());

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
    /// <param name="instance"> Instance to load the data into, if the field is not static. </param>
    /// <param name="keyPostfix"> Postfix to append to the key. Used for instance-specific keys. </param>
    /// <returns> True if the data was loaded successfully, false otherwise. </returns>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static bool LoadData(FieldInfo field, object? instance = null, string? keyPostfix = null)
    {
        if (ModDataFields[field].BaseKey == null)
        {
            LethalModDataLib.Logger?.LogWarning(
                $"Field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName} has no base key!");
            return false;
        }

        var key = GetFieldKey(field, keyPostfix);
        var saveLocation = ModDataFields[field].SaveLocation;

        var value = LoadData<object>(key, saveLocation: saveLocation, autoAddGuid: false);

        field.SetValue(instance, value);
        return true;
    }


    /// <summary>
    ///     Loads data based on the ModDataAttribute attached to the property.
    /// </summary>
    /// <param name="property"> Property to load. </param>
    /// <param name="instance"> Instance to load the data into, if the property is not static. </param>
    /// <param name="keyPostfix"> Postfix to append to the key. Used for instance-specific keys. </param>
    /// <returns> True if the data was loaded successfully, false otherwise. </returns>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static bool LoadData(PropertyInfo property, object? instance = null, string? keyPostfix = null)
    {
        if (ModDataProperties[property].BaseKey == null)
        {
            LethalModDataLib.Logger?.LogWarning(
                $"Property {property.Name} from {property.DeclaringType?.AssemblyQualifiedName} has no base key!");
            return false;
        }

        var key = GetPropertyKey(property, keyPostfix);
        var saveLocation = ModDataProperties[property].SaveLocation;

        var value = LoadData<object>(key, saveLocation: saveLocation, autoAddGuid: false);

        property.SetValue(instance, value);
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
            var guid = ModDataHelper.GetCallingPluginGuid(Assembly.GetCallingAssembly());

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
    /// <param name="instance"> Instance to load the data into, if the field is not static. </param>
    /// <param name="keyPostfix"> Postfix to append to the key. Used for instance-specific keys. </param>
    /// <returns> True if the data was saved successfully, false otherwise. </returns>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static bool SaveData(FieldInfo field, object? instance = null, string? keyPostfix = null)
    {
        if (!ModDataFields.ContainsKey(field))
        {
            LethalModDataLib.Logger?.LogWarning(
                $"Field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName} is not registered!");
            return false;
        }

        if (ModDataFields[field].BaseKey == null)
        {
            LethalModDataLib.Logger?.LogWarning(
                $"Field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName} has no base key!");
            return false;
        }

        var key = GetFieldKey(field, keyPostfix);
        var saveLocation = ModDataFields[field].SaveLocation;

        var value = field.GetValue(instance);

        return SaveData(value, key, saveLocation, false);
    }

    /// <summary>
    ///     Saves data based on the ModDataAttribute attached to the property.
    /// </summary>
    /// <param name="field"> Field to save. </param>
    /// <param name="instance"> Instance to load the data into, if the property is not static. </param>
    /// <param name="keyPostfix"> Postfix to append to the key. Used for instance-specific keys. </param>
    /// <returns> True if the data was saved successfully, false otherwise. </returns>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static bool SaveData(PropertyInfo property, object? instance = null, string? keyPostfix = null)
    {
        if (!ModDataProperties.ContainsKey(property))
        {
            LethalModDataLib.Logger?.LogWarning(
                $"Property {property.Name} from {property.DeclaringType?.AssemblyQualifiedName} is not registered!");
            return false;
        }

        if (ModDataProperties[property].BaseKey == null)
        {
            LethalModDataLib.Logger?.LogWarning(
                $"Property {property.Name} from {property.DeclaringType?.AssemblyQualifiedName} has no base key!");
            return false;
        }

        var key = GetPropertyKey(property, keyPostfix);
        var saveLocation = ModDataProperties[property].SaveLocation;

        var value = property.GetValue(instance);

        return SaveData(value, key, saveLocation, false);
    }

    #endregion
}