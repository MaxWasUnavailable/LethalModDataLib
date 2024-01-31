using System.Reflection;

namespace LethalModDataLib.Features;

public static class ModDataHelper
{
    /// <summary>
    ///     Gets the field info for a field. Used to manually handle saving and/or loading of an attributed field.
    /// </summary>
    /// <param name="obj"> Object to get the field info from. </param>
    /// <param name="fieldName"> Name of the field to get the field info for. </param>
    /// <returns> The field info for the field. </returns>
    public static FieldInfo? GetFieldInfo(this object obj, string fieldName)
    {
        return obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                                 BindingFlags.Static);
    }

    /// <summary>
    ///     Gets the current save file name.
    /// </summary>
    /// <returns> The current save file name. </returns>
    /// <remarks> When used too early, this will return the last save file used by the player. </remarks>
    public static string GetCurrentSaveFileName()
    {
        return GameNetworkManager.Instance.currentSaveFileName;
    }

    /// <summary>
    ///     Gets the general save file name.
    /// </summary>
    /// <returns> The general save file name. </returns>
    public static string GetGeneralSaveFileName()
    {
        return GameNetworkManager.generalSaveDataName;
    }
}