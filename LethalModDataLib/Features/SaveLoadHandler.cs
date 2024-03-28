using System;
using System.Reflection;
using LethalModDataLib.Enums;
using LethalModDataLib.Helpers;
using LethalModDataLib.Interfaces;

namespace LethalModDataLib.Features;

/// <summary>
///     Class to handle saving and loading of mod data.
/// </summary>
public static class SaveLoadHandler
{
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
    /// <remarks> This method is not recommended for use in most cases. Use the other overloads instead. </remarks>
    private static T? LoadData<T>(string key, string fileName, T? defaultValue = default)
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
    public static T? LoadData<T>(string key, SaveLocation saveLocation = SaveLocation.CurrentSave,
        T? defaultValue = default, bool autoAddGuid = true)
    {
        // We prevent loading from the current save file if we're not the host. Mods should sync values to clients in this case.
        if (saveLocation == SaveLocation.CurrentSave && !ModDataHelper.IsHost())
        {
            LethalModDataLib.Logger?.LogDebug($"Not loading {key} from current save file as we're not the host!");
            return defaultValue;
        }

        // ReSharper disable once InvertIf
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
    ///     Loads data based on the ModDataAttribute attached to the field or property.
    /// </summary>
    /// <param name="modDataKey"> ModDataKey to load. </param>
    /// <returns> True if the data was loaded successfully, false otherwise. </returns>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static bool LoadData(IModDataKey modDataKey)
    {
        if (!ModDataHandler.ModDataValues.TryGetValue(modDataKey, out var modDataValue))
        {
            LethalModDataLib.Logger?.LogWarning(
                $"Field or property {modDataKey.Name} from {modDataKey.AssemblyQualifiedName} has no registered mod data attribute! " +
                $"If this is an instance of a class, make sure to register it using ModDataHandler.RegisterInstance!");
            return false;
        }

        var key = modDataKey.ToES3KeyString();
        var saveLocation = modDataValue.SaveLocation;

        // If the field or property has a value, we'll use it as default in case no saved moddata is found
        modDataKey.TryGetValue(out var currentValue);

        var value = LoadData(key, saveLocation, autoAddGuid: false, defaultValue: currentValue);

        try
        {
            LethalModDataLib.Logger?.LogDebug(
                $"Loaded value for property or field {modDataKey.Name}: {value}");
        }
        catch (Exception e)
        {
            // ignored in case the value can't be converted to a string
        }

        if (modDataKey.TrySetValue(value))
            return true;

        LethalModDataLib.Logger?.LogDebug(
            $"Failed to set value for field or property {modDataKey.Name} from {modDataKey.AssemblyQualifiedName}! Does it not have a setter?");
        return false;
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
    /// <remarks> This method is not recommended for use in most cases. Use the other overloads instead. </remarks>
    private static bool SaveData<T>(T? data, string key, string fileName)
    {
        VerifyKeyAndFileName(key, fileName);

        fileName += ".moddata";

        try
        {
            LethalModDataLib.Logger?.LogDebug($"Saving data to file {fileName} with key {key}...");

            try
            {
                LethalModDataLib.Logger?.LogDebug($"Data to save: {data}");
            }
            catch (Exception e)
            {
                // ignored in case the data can't be converted to a string
            }

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
    ///     key. Highly recommended to keep this set to true unless you fully understand the implications.
    /// </param>
    /// <typeparam name="T"> Type of the data to save. </typeparam>
    /// <returns> True if the data was saved successfully, false otherwise. </returns>
    /// <exception cref="ArgumentOutOfRangeException"> Thrown if the save location is invalid. </exception>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static bool SaveData<T>(T? data, string key, SaveLocation saveLocation = SaveLocation.CurrentSave,
        bool autoAddGuid = true)
    {
        // We prevent saving to the current save file if we're not the host, since that would impact our own (unrelated) client save files
        if (saveLocation == SaveLocation.CurrentSave && !ModDataHelper.IsHost())
        {
            LethalModDataLib.Logger?.LogDebug($"Not saving {key} to current save file as we're not the host!");
            return false;
        }

        // ReSharper disable once InvertIf
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
    ///     Saves data based on the ModDataAttribute attached to the field or property.
    /// </summary>
    /// <param name="modDataKey"> ModDataKey to save. </param>
    /// <returns> True if the data was saved successfully, false otherwise. </returns>
    /// <exception cref="ArgumentException"> Thrown if the key or file name is null or empty. </exception>
    public static bool SaveData(IModDataKey modDataKey)
    {
        if (!ModDataHandler.ModDataValues.TryGetValue(modDataKey, out var modDataValue))
        {
            LethalModDataLib.Logger?.LogWarning(
                $"Field or property {modDataKey.Name} from {modDataKey.AssemblyQualifiedName} has no registered mod data attribute! " +
                $"If this is an instance of a class, make sure to register it using ModDataHandler.RegisterInstance!");
            return false;
        }

        var key = modDataKey.ToES3KeyString();
        var saveLocation = modDataValue.SaveLocation;

        if (modDataKey.TryGetValue(out var value))
            return SaveData(value, key, saveLocation, false);

        LethalModDataLib.Logger?.LogDebug(
            $"Failed to get value from field or property {modDataKey.Name} from {modDataKey.AssemblyQualifiedName}! Does it not have a getter?");
        return false;
    }

    #endregion
}