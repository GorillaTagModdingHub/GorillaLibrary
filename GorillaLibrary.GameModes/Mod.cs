using GorillaLibrary.GameModes;
using GorillaLibrary.GameModes.Attributes;
using GorillaLibrary.GameModes.Behaviours;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(Mod), "GorillaLibrary.GameModes", "1.0.0", "dev9998")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
[assembly: MelonAdditionalDependencies("GorillaLibrary")]

namespace GorillaLibrary.GameModes;

[ModdedGamemode]
internal class Mod : MelonMod
{
    public override void OnEarlyInitializeMelon()
    {
        Events.Game.OnGameInitialized.Subscribe(OnGameInitialized);
    }

    public void OnGameInitialized()
    {
        Object.DontDestroyOnLoad(new GameObject("Utilla", typeof(NetworkController), typeof(GameModeManager)));
    }

    [ModdedGamemodeJoin]
    public void OnModdedJoin()
    {
        LoggerInstance.Msg("ModdedGamemodeJoin");
    }

    [ModdedGamemodeLeave]
    public void OnModdedLeave()
    {
        LoggerInstance.Msg("ModdedGamemodeLeave");
    }
}
