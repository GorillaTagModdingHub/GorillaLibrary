namespace GorillaLibrary.Patches;

internal class GameOverlayPatch
{
    public static void Postfix(bool ___isGameOverlayActive)
    {
        Events.Player.OnGameOverlayActivation.Invoke(___isGameOverlayActive);
    }
}
