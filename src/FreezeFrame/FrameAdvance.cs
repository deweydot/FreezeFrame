using System;
using System.Linq;
using System.Reflection;

using HarmonyLib;
using UnityEngine;

namespace FreezeFrame {
    public static class FrameController {
        private static int stepFrames = -1;
        public static float logicalTime = 0f;
        public static float logicalDeltaTime = 0.008f;
        
        public static void ApplyPatch(Harmony harmony) {
            harmony.CreateClassProcessor(typeof(TimePatch)).Patch();
            harmony.CreateClassProcessor(typeof(DeltaTimePatch)).Patch();
            PatchAssembly(harmony);
        }
        
        public static void Update() {
            if (stepFrames > 0){
                stepFrames--;
                Time.timeScale = 0f;
            }
        }

        public static void LateUpdate() {
            if (stepFrames > 0){
                Physics.Simulate(0.008f);
                logicalTime += logicalDeltaTime;
            }
        }

        public static void Enable() {
            if (IsActive()) return;
            stepFrames = 0;
            Time.captureDeltaTime = 0.008f;
            Physics.simulationMode = SimulationMode.Script;
            Time.timeScale = 0f;
        }

        public static void Disable() {
            if (!IsActive()) return;
            stepFrames = -1;
            Time.captureDeltaTime = 0f;
            Physics.simulationMode = SimulationMode.FixedUpdate;
            Time.timeScale = 1f;
        }

        public static void Advance(int count) {
            if (!IsActive()) return;
            stepFrames += count;
            Time.timeScale = 1f;
        }

        private static void PatchAssembly(Harmony harmony) {
            var gameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == "Assembly-CSharp");
            foreach (var type in gameAssembly.GetTypes()) {
                if (!typeof(MonoBehaviour).IsAssignableFrom(type)) continue;
                foreach (var methodName in new[] { "Update", "FixedUpdate", "LateUpdate" }) {
                    var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method == null) continue;
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(FrameController), nameof(FreezeGate)));
                }
            }
        }

        public static bool FreezeGate() => stepFrames != 0;

        public static bool IsActive() => stepFrames >= 0;
    }

    [HarmonyPatch(typeof(Time), nameof(Time.time), MethodType.Getter)]
    static class TimePatch {
        static bool Prefix(ref float __result) {
            if (!FrameController.IsActive()) return true;
            __result = FrameController.logicalTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.deltaTime), MethodType.Getter)]
    static class DeltaTimePatch {
        static bool Prefix(ref float __result) {
            if (!FrameController.IsActive()) return true;
            __result = FrameController.logicalDeltaTime;
            return false;
        }
    }
}