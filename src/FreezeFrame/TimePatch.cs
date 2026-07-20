using HarmonyLib;
using UnityEngine;

namespace FreezeFrame {
    [HarmonyPatch(typeof(Time), nameof(Time.time), MethodType.Getter)]
    static class Time_Time_Patch {
        static bool Prefix(ref float __result) {
            if (!FreezeFrame.IsActive()) return true;
            __result = FreezeFrame.logicalTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.deltaTime), MethodType.Getter)]
    static class Time_DeltaTime_Patch {
        static bool Prefix(ref float __result) {
            if (!FreezeFrame.IsActive()) return true;
            __result = FreezeFrame.logicalDeltaTime;
            return false;
        }
    }
}