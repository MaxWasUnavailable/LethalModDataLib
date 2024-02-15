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
    public static Dictionary<FieldInfo, ModDataAttribute> ModDataFields { get; } = new();
    public static Dictionary<PropertyInfo, ModDataAttribute> ModDataProperties { get; } = new();

    /// <summary>
    ///     Gets the moddata key for a field registered with the ModDataAttribute.
    /// </summary>
    /// <param name="field"> Field to get the moddata key for. </param>
    /// <param name="keyPostfix"> Postfix to append to the key. Used for instance-specific keys. </param>
    /// <returns> The moddata key for the field. </returns>
    public static string GetFieldKey(FieldInfo field, string? keyPostfix = null)
    {
        if (!ModDataFields.TryGetValue(field, out var modDataAttribute))
            throw new ArgumentException("Field is not registered with the ModDataAttribute!");
        
        var key = modDataAttribute.BaseKey + ".";

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
    public static string GetPropertyKey(PropertyInfo property, string? keyPostfix = null)
    {
        if (!ModDataProperties.TryGetValue(property, out var modDataAttribute))
            throw new ArgumentException("Property is not registered with the ModDataAttribute!");
        
        var key = modDataAttribute.BaseKey + ".";

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
        if (!SaveLoadHandler.SaveData(field))
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to save field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName}!");
    }

    private static void HandleSaveProperty(PropertyInfo property)
    {
        if (!SaveLoadHandler.SaveData(property))
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to save property {property.Name} from {property.DeclaringType?.AssemblyQualifiedName}!");
    }

    private static void HandleLoadField(FieldInfo field)
    {
        if (!SaveLoadHandler.LoadData(field))
            LethalModDataLib.Logger?.LogWarning(
                $"Failed to load field {field.Name} from {field.DeclaringType?.AssemblyQualifiedName}!");
    }

    private static void HandleLoadProperty(PropertyInfo property)
    {
        if (!SaveLoadHandler.LoadData(property))
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
}