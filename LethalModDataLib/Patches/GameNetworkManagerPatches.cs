using HarmonyLib;
using LethalModDataLib.Events;

// ReSharper disable InconsistentNaming

namespace LethalModDataLib.Patches;

/// <summary>
///     Patches for <see cref="GameNetworkManager" />.
/// </summary>
[HarmonyPatch(typeof(GameNetworkManager))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal static class GameNetworkManagerPatches
{
    /// <summary>
    ///     Triggers after the game is saved.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameNetworkManager.SaveGame))]
    private static void SaveGamePostfix(GameNetworkManager __instance)
    {
        SaveLoadEvents.OnPostSaveGame(StartOfRound.Instance.isChallengeFile, __instance.currentSaveFileName);
    }

    /// <summary>
    ///     Called after the game resets its saved game values. (This only happens after a game over)
    /// </summary>
    /// <param name="__instance"> The <see cref="GameNetworkManager" /> instance. </param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameNetworkManager.ResetSavedGameValues))]
    private static void PostResetSavedGameValues(GameNetworkManager __instance)
    {
        SaveLoadEvents.OnPostResetSavedGameValues();
    }
}