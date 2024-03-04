namespace LethalModDataLib.Events;

/// <summary>
///     Miscellaneous events.
/// </summary>
public static class MiscEvents
{
    /// <summary>
    ///     Called after the game has been initialized.
    /// </summary>
    public delegate void PostInitializeGameEventHandler();

    /// <summary>
    ///     Called after the main menu is initialized.
    /// </summary>
    public static event PostInitializeGameEventHandler? PostInitializeGameEvent;

    /// <summary>
    ///     Called after the main menu is initialized.
    /// </summary>
    internal static void OnPostInitializeGame()
    {
        PostInitializeGameEvent?.Invoke();
    }
}