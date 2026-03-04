using GorillaLibrary.Modding;
using GorillaLibrary.Modding.Behaviours;
using MelonLoader;
using System;
using UnityEngine;

[assembly: MelonInfo(typeof(Mod), "GorillaLibrary.GameModes", "1.0.0", "dev9998")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
[assembly: MelonAdditionalDependencies("GorillaLibrary")]

namespace GorillaLibrary.Modding;

internal class Mod : MelonMod
{
    public override void OnEarlyInitializeMelon()
    {
        GameModeEvents.GameInitialized += OnGameInitialized;
    }

    public void OnGameInitialized(object sender, EventArgs args)
    {
        UnityEngine.Object.DontDestroyOnLoad(new GameObject("Utilla", typeof(NetworkController), typeof(GameModeManager)));
    }
}
