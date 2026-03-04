using GorillaLibrary.Modding.Behaviours;
using HarmonyLib;
using System;
using System.Reflection;

namespace GorillaLibrary.Modding.Patches
{
    [HarmonyPatch(typeof(GameModeSelectorButtonLayout))]
    internal class GameModeSelectorPatch
    {
        [HarmonyPatch("OnEnable"), HarmonyPrefix]
        public static bool OnEnablePatch(GameModeSelectorButtonLayout __instance)
        {
            if ((GorillaPressableButton)AccessTools.Field(typeof(GameModeSelectorButtonLayout), "superToggleButton").GetValue(__instance) is GorillaPressableButton superToggleButton)
            {
                EventInfo eventInfo = superToggleButton.GetType().GetEvent("onPressed", AccessTools.all);
                Delegate handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, __instance, AccessTools.Method(__instance.GetType(), "_OnPressedSuperToggleButton"));
                eventInfo.AddEventHandler(superToggleButton, handler);
            }

            AccessTools.Method(__instance.GetType(), "SetupButtons").Invoke(__instance, null);

            if (__instance.TryGetComponent(out GameModeSelector selector))
            {
                selector.CheckGameMode();
                selector.ShowPage();
                return false;
            }

            __instance.AddComponent<GameModeSelector>();
            return false;
        }

        [HarmonyPatch("SetupButtons"), HarmonyPrefix]
        public static void SetupButtonsPrefix(GameModeSelectorButtonLayout __instance)
        {
            // NetworkSystem.Instance.OnJoinedRoomEvent -= __instance.SetupButtons;
        }

        [HarmonyPatch("SetupButtons"), HarmonyPostfix]
        public static void SetupButtonsPostfix(GameModeSelectorButtonLayout __instance)
        {
            if (!__instance.TryGetComponent(out GameModeSelector selector)) return;
            selector.OnSelectorSetup();
        }
    }
}