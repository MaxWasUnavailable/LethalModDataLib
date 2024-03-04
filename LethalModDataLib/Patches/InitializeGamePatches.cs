using HarmonyLib;
using LethalModDataLib.Events;

namespace LethalModDataLib.Patches;

/// <summary>
///     Patches for the InitializeGame class.
///     For events related to the game initialization.
/// </summary>
[HarmonyPatch(typeof(InitializeGame))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal static class InitializeGamePatches
{
    /// <summary>
    ///     Triggers after the main menu is initialized.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(InitializeGame.Start))]
    private static void StartPostfix()
    {
        MiscEvents.OnPostInitializeGame();
    }
}