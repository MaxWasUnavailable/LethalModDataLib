using BepInEx;
using BepInEx.Logging;
using LethalEventsLib.Events;
using LethalModDataLib.Enums;
using LethalModDataLib.Features;

namespace LethalModDataLib;

[BepInDependency(LethalEventsLib.PluginInfo.PLUGIN_GUID, LethalEventsLib.PluginInfo.PLUGIN_VERSION)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class LethalModDataLib : BaseUnityPlugin
{
    private const string ModVersionKey = "LMDLVersion";
    internal new static ManualLogSource? Logger { get; private set; }
    public static LethalModDataLib? Instance { get; private set; }

    private void Awake()
    {
        // Set instance
        Instance = this;

        // Init logger
        Logger = base.Logger;

        // Hook up initialisation event
        SystemEvents.PostInitializeGameEvent += OnGameInitialized;

        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    /// <summary>
    ///     Check if the version of LethalModDataLib has changed since the last time the game was run.
    ///     Save the current version to general mod data.
    ///     This is useful for debugging & bug reports.
    /// </summary>
    private static void VersionCheck()
    {
        // Load LethalModDataLib version from general mod data
        var version = SaveLoadHandler.LoadData<string>(ModVersionKey, SaveLocation.GeneralSave);

        // Check if version is null
        if (string.IsNullOrEmpty(version))
        {
            Logger?.LogInfo(
                "No saved LethalModDataLib version found. " +
                "This is normal if this is the first time you are running the game with LethalModDataLib installed.");
        }
        else
        {
            // Check if version is equal to current version
            if (version == PluginInfo.PLUGIN_VERSION)
            {
                Logger?.LogDebug(
                    $"LethalModDataLib version ({PluginInfo.PLUGIN_VERSION}) matches last saved version ({version}).");
            }
            else
            {
                SaveLoadHandler.SaveData(version ?? string.Empty, ModVersionKey + "_old", SaveLocation.GeneralSave);
                Logger?.LogWarning(
                    $"Mismatch between last saved LethalModDataLib version ({version})" +
                    $"and current version ({PluginInfo.PLUGIN_VERSION})." +
                    $"This is normal if you have updated LethalModDataLib." +
                    $"Make sure to check the changelog for breaking changes.");
            }
        }

        // Save LethalModDataLib version to general mod data
        SaveLoadHandler.SaveData<string>(PluginInfo.PLUGIN_VERSION, ModVersionKey, SaveLocation.GeneralSave);
    }

    private static void OnGameInitialized()
    {
        // Initialise ModDataHandler after all other plugins have loaded
        ModDataHandler.Initialise();

        // Unhook initialisation event
        SystemEvents.PostInitializeGameEvent -= OnGameInitialized;

        // Check version
        VersionCheck();
    }
}