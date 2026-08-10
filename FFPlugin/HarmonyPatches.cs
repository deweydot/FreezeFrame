using HarmonyLib;
using UnityEngine;

namespace FreezeFrame
{
    [HarmonyPatch(typeof(Time), nameof(Time.time), MethodType.Getter)]
    static class TimePatch
    {
        static bool Prefix(ref float __result)
        {
            if (FrameController.state == FrameState.Continuous) return true;
            __result = FrameController.logicalTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.deltaTime), MethodType.Getter)]
    static class DeltaTimePatch
    {
        static bool Prefix(ref float __result)
        {
            if (FrameController.state == FrameState.Continuous) return true;
            if (FrameController.state == FrameState.UpdateBothStep) __result = FrameController.logicalDeltaTime;
            else __result = 0;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputManager), nameof(InputManager.SaveBindings))]
    static class SaveBindingsPatch
    {
        static bool Prefix()
        {
            return FrameController.state == FrameState.Continuous;
        }
    }

    [HarmonyPatch(typeof(PrefsManager), "CommitPrefs")]
    static class CommitPrefsPatch
    {
        static bool Prefix()
        {
            return FrameController.state == FrameState.Continuous;
        }
    }
}
