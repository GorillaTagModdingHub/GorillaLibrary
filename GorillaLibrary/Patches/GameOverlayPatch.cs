using static GorillaLibrary.Events.PlayerEvents;

namespace GorillaLibrary.Patches
{
    internal class GameOverlayPatch
    {
        public static void Postfix(bool ___isGameOverlayActive)
        {
            GorillaMod.Bus.Publish(new GameOverlayActivationEvent(___isGameOverlayActive));
        }
    }
}
