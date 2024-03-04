using HarmonyLib;
using LethalModDataLib.Events;

// ReSharper disable InconsistentNaming

namespace LethalModDataLib.Patches;

/// <summary>
///     Patches for <see cref="ES3" />.
///     For events related to deleting save files.
/// </summary>
[HarmonyPatch(typeof(ES3))]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal static class ES3Patches
{
    /// <summary>
    ///     Triggers after a file is deleted through ES3.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ES3.DeleteFile))]
    private static void DeleteFilePostfix(string filePath)
    {
        if (filePath.Contains(".moddata"))
            return;

        LethalModDataLib.Logger?.LogError($"File {filePath} deleted!");

        SaveLoadEvents.OnPostDeleteSave(filePath);
    }
}