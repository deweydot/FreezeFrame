using System;
using System.Linq;
using System.Reflection;

using HarmonyLib;
using UnityEngine;

namespace FreezeFrame {
    static class FrameController {
        public static float logicalTime = 0f;
        public static float logicalDeltaTime = 0.008f;
        private static InputDevice input;
        
        public static void Init(Harmony harmony) {
            harmony.CreateClassProcessor(typeof(TimePatch)).Patch();
            harmony.CreateClassProcessor(typeof(DeltaTimePatch)).Patch();
            PatchAssembly(harmony);
        }

        public static void Update() {
            if (input == null) input = new InputDevice();
            if (FreezeFrame.state == FrameState.UpdateOnlyStep || 
                FreezeFrame.state == FrameState.UpdateBothStep) {
                Enable();
            }
        }

        public static void LateUpdate() {
            if (FreezeFrame.state == FrameState.UpdateBothStep) {
                Physics.Simulate(0.008f);
                logicalTime += logicalDeltaTime;
            }
        }

        public static void Enable() {
            if (FreezeFrame.state == FrameState.Suspended) return;
            FreezeFrame.state = FrameState.Suspended;
            Time.captureDeltaTime = 0.008f;
            Physics.simulationMode = SimulationMode.Script;
            Time.timeScale = 0f;
            input.Enable();
        }

        public static void Disable() {
            if (FreezeFrame.state == FrameState.Continous) return;
            FreezeFrame.state = FrameState.Continous;
            Time.captureDeltaTime = 0f;
            Physics.simulationMode = SimulationMode.FixedUpdate;
            Time.timeScale = 1f;
            input.Disable();
        }

        public static void Advance(bool fixedUpdate, InputState? state) {
            if (FreezeFrame.state == FrameState.Continous) return;
            FreezeFrame.state = fixedUpdate ? FrameState.UpdateBothStep : FrameState.UpdateOnlyStep;
            Time.timeScale = 1f;
            if (state != null) input.queueState(state.Value.kb, state.Value.m);
        }

        private static void PatchAssembly(Harmony harmony) {
            var gameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == "Assembly-CSharp");
            foreach (var type in gameAssembly.GetTypes()) {
                if (!typeof(MonoBehaviour).IsAssignableFrom(type)) continue;
                var updateMethod = type.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var fixedUpdateMethod = type.GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var lateUpdateMethod = type.GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (updateMethod != null) harmony.Patch(updateMethod, prefix: new HarmonyMethod(typeof(FrameController), nameof(UpdateGate)));
                if (fixedUpdateMethod != null) harmony.Patch(fixedUpdateMethod, prefix: new HarmonyMethod(typeof(FrameController), nameof(FixedUpdateGate)));
                if (lateUpdateMethod != null) harmony.Patch(lateUpdateMethod, prefix: new HarmonyMethod(typeof(FrameController), nameof(UpdateGate)));
            }
        }

        public static bool UpdateGate() => FreezeFrame.state != FrameState.Suspended;
        public static bool FixedUpdateGate() => FreezeFrame.state == FrameState.Continous || FreezeFrame.state == FrameState.UpdateBothStep;
    }

    [HarmonyPatch(typeof(Time), nameof(Time.time), MethodType.Getter)]
    static class TimePatch {
        static bool Prefix(ref float __result) {
            if (FreezeFrame.state == FrameState.Continous) return true;
            __result = FrameController.logicalTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.deltaTime), MethodType.Getter)]
    static class DeltaTimePatch {
        static bool Prefix(ref float __result) {
            if (FreezeFrame.state == FrameState.Continous) return true;
            if (FreezeFrame.state == FrameState.UpdateBothStep) __result = FrameController.logicalDeltaTime;
            else __result = 0;
            return false;
        }
    }
}