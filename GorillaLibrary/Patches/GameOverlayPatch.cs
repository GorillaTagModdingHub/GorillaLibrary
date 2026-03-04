using static GorillaLibrary.Events.PlayerEvents;

namespace GorillaLibrary.Patches
{
    internal class GameOverlayPatch
    {
        public static void Postfix(GorillaTagger __instance, bool ___isGameOverlayActive)
        {
            GorillaLibrary.Instance.Bus.Publish(new GameOverlayActivationEvent(___isGameOverlayActive));
        }
    }
}
