namespace GorillaLibrary.Models;

internal class InternalRoom
{
    /// <summary>
    /// Whether or not the room is private.
    /// </summary>
    public bool IsPrivate { get; set; }

    /// <summary>
    /// The gamemode that the current lobby is 
    /// </summary>
    public string Gamemode { get; set; }
}
