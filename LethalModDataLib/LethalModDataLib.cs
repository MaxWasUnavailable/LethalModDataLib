using BepInEx;
using BepInEx.Logging;
using LethalEventsLib.Events;
using LethalModDataLib.Features;

namespace LethalModDataLib;

[BepInDependency(LethalEventsLib.PluginInfo.PLUGIN_GUID)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class LethalModDataLib : BaseUnityPlugin
{
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

    private static void OnGameInitialized()
    {
        // Initialise ModDataHandler
        ModDataHandler.Initialise();
    }
}