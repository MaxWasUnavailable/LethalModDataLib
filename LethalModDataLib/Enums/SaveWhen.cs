namespace LethalModDataLib.Enums;

/// <summary>
///     Enum to specify when to save fields marked with <see cref="Attributes.ModDataAttribute" />.
/// </summary>
public enum SaveWhen
{
    /// <summary>
    ///     Saving is left to the modder to handle.
    /// </summary>
    Manual,

    /// <summary>
    ///     Save when the game autosaves.
    /// </summary>
    OnAutoSave,

    /// <summary>
    ///     Save when the game saves.
    /// </summary>
    /// <remarks> This is at least as frequent as OnAutoSave, since an autosave triggers the save event. </remarks>
    OnSave
}