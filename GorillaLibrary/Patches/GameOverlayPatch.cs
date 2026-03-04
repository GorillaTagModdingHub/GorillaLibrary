using GorillaLibrary.Events;

namespace GorillaLibrary.Patches
{
    internal class GameOverlayPatch
    {
        public static void Postfix(GorillaTagger __instance, bool ___isGameOverlayActive)
        {
            PlayerEvents.OnGameOverlayActivation?.InvokeSafe(___isGameOverlayActive);
        }
    }
}
