namespace LethalModDataLib.Events;

/// <summary>
///     Save / load events.
/// </summary>
public static class SaveLoadEvents
{
    /// <summary>
    ///     Called after the game has been autosaved.
    /// </summary>
    /// <remarks> Autosaves are triggered when the ship returns to orbit, right after all players have been revived. </remarks>
    public delegate void PostAutoSaveEventHandler(bool isChallenge, string saveFileName);

    /// <summary>
    ///     Called after a save file is deleted.
    /// </summary>
    public delegate void PostDeleteSaveEventHandler(string saveFileName);

    /// <summary>
    ///     Called after the game has been initialized.
    /// </summary>
    public delegate void PostLoadGameEventHandler(bool isChallenge, string saveFileName);

    /// <summary>
    ///     Called after the game resets its saved game values. (This only happens after a game over)
    /// </summary>
    public delegate void PostResetSavedGameValuesEventHandler();

    /// <summary>
    ///     Called after the game has been saved.
    /// </summary>
    public delegate void PostSaveGameEventHandler(bool isChallenge, string saveFileName);

    /// <summary>
    ///     Called after the game is saved.
    /// </summary>
    public static event PostSaveGameEventHandler? PostSaveGameEvent;

    /// <summary>
    ///     Called after the game has been saved.
    /// </summary>
    /// <param name="isChallenge"> True if the save is a challenge save. </param>
    /// <param name="saveFileName"> The name of the save file. </param>
    internal static void OnPostSaveGame(bool isChallenge, string saveFileName)
    {
        PostSaveGameEvent?.Invoke(isChallenge, saveFileName);
    }

    /// <summary>
    ///     Called after the game autosaves.
    /// </summary>
    /// <remarks> Autosaves are triggered when the ship returns to orbit, right after all players have been revived. </remarks>
    public static event PostAutoSaveEventHandler? PostAutoSaveEvent;

    /// <summary>
    ///     Called after the game autosaves.
    /// </summary>
    /// <param name="isChallenge"> True if the save is a challenge save. </param>
    /// <param name="saveFileName"> The name of the save file. </param>
    internal static void OnPostAutoSave(bool isChallenge, string saveFileName)
    {
        PostAutoSaveEvent?.Invoke(isChallenge, saveFileName);
    }

    /// <summary>
    ///     Called after a save file is loaded or started for the first time.
    /// </summary>
    public static event PostLoadGameEventHandler? PostLoadGameEvent;

    internal static void OnPostLoadGame(bool isChallenge, string saveFileName)
    {
        PostLoadGameEvent?.Invoke(isChallenge, saveFileName);
    }

    /// <summary>
    ///     Called after a save file is deleted.
    /// </summary>
    public static event PostDeleteSaveEventHandler? PostDeleteSaveEvent;

    /// <summary>
    ///     Called after a save file is deleted.
    /// </summary>
    internal static void OnPostDeleteSave(string saveFileName)
    {
        PostDeleteSaveEvent?.Invoke(saveFileName);
    }

    /// <summary>
    ///     Called after the game resets its saved game values. (This only happens after a game over)
    /// </summary>
    public static event PostResetSavedGameValuesEventHandler? PostResetSavedGameValuesEvent;

    /// <summary>
    ///     Called after the game resets its saved game values. (This only happens after a game over)
    /// </summary>
    internal static void OnPostResetSavedGameValues()
    {
        PostResetSavedGameValuesEvent?.Invoke();
    }
}