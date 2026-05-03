using GorillaLibrary.Behaviours;

namespace GorillaLibrary.Utilities;

public static class ConductBoardUtility
{
    /// <summary>
    /// Adds a new page to the Code of Conduct board, the board supports rich text.
    /// </summary>
    public static void AddEntry(string title, string body)
    {
        ConductBoardManager.Instance.AddEntry(title, body);
    }
}