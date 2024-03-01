namespace LethalModDataLib.Enums;

/// <summary>
///     Enum to specify what location to save and load data.
/// </summary>
public enum SaveLocation
{
    /// <summary>
    ///     Target the current save file.
    /// </summary>
    CurrentSave,

    /// <summary>
    ///     Target the general, save-agnostic file.
    /// </summary>
    GeneralSave
}