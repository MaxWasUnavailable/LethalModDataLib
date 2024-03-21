using System;

namespace LethalModDataLib.Enums;

/// <summary>
///     Enum to specify when to save fields/properties marked with <see cref="Attributes.ModDataAttribute" />.
/// </summary>
[Flags]
public enum SaveWhen
{
    /// <summary>
    ///     Saving is left to the modder to handle.
    /// </summary>
    Manual = 0,

    /// <summary>
    ///     Save when the game autosaves.
    /// </summary>
    OnAutoSave = 1,

    /// <summary>
    ///     Save when the game saves.
    /// </summary>
    /// <remarks> This is at least as frequent as OnAutoSave, since an autosave triggers the save event. </remarks>
    OnSave = 2
}