using HarmonyLib;
using LethalModDataLib.Events;

// ReSharper disable InconsistentNaming

namespace LethalModDataLib.Patches;

/// <summary>
///     Patches for <see cref="StartOfRound" />.
/// </summary>
[HarmonyPatch(typeof(StartOfRound))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal static class StartOfRoundPatches
{
    /// <summary>
    ///     Called after the game autosaves.
    /// </summary>
    /// <param name="__instance"> The <see cref="StartOfRound" /> instance. </param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(StartOfRound.AutoSaveShipData))]
    private static void PostAutoSaveShipData(StartOfRound __instance)
    {
        SaveLoadEvents.OnPostAutoSave(__instance.isChallengeFile,
            GameNetworkManager.Instance.currentSaveFileName);
    }

    /// <summary>
    ///     Called after the game loads.
    /// </summary>
    /// <param name="__instance"> The <see cref="StartOfRound" /> instance. </param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(StartOfRound.Start))]
    private static void PostStart(StartOfRound __instance)
    {
        SaveLoadEvents.OnPostLoadGame(__instance.isChallengeFile,
            GameNetworkManager.Instance.currentSaveFileName);
    }
}