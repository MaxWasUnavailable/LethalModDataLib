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
}