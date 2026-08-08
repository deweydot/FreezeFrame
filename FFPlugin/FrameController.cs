using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FreezeFrame
{
    class FrameController
    {
        public static float logicalTime = 0f;
        public static float logicalDeltaTime = 0.008f; // 1/125 seconds
        private VirtualInput input;
        private AccessTools.FieldRef<TimeController, float> timeScaleRef;
        private AccessTools.FieldRef<TimeController, float> timeScaleModifierRef;

        public FrameController(Harmony harmony)
        {
            harmony.CreateClassProcessor(typeof(TimePatch)).Patch();
            harmony.CreateClassProcessor(typeof(DeltaTimePatch)).Patch();
            PatchAssembly(harmony);
            timeScaleRef = AccessTools.FieldRefAccess<TimeController, float>("timeScale");
            timeScaleModifierRef = AccessTools.FieldRefAccess<TimeController, float>("timeScaleModifier");
        }

        public void Update()
        {
            if (input == null) input = new VirtualInput();
            if (Plugin.state == FrameState.UpdateOnlyStep ||
                Plugin.state == FrameState.UpdateBothStep)
            {
                Enable();
            }
        }

        public void LateUpdate()
        {
            if (Plugin.state == FrameState.UpdateBothStep)
            {
                Physics.Simulate(0.008f);
                logicalTime += logicalDeltaTime;
            }
        }

        public void Enable()
        {
            Plugin.state = FrameState.Suspended;
            Time.captureDeltaTime = 0.008f;
            Physics.simulationMode = SimulationMode.Script;
            Time.timeScale = 0f;
            input.Enable();
        }

        public void Disable()
        {
            Plugin.state = FrameState.Continous;
            Time.captureDeltaTime = 0f;
            Physics.simulationMode = SimulationMode.FixedUpdate;
            Time.timeScale = timeScaleRef(MonoSingleton<TimeController>.Instance) * timeScaleModifierRef(MonoSingleton<TimeController>.Instance);
            input.Disable();
        }

        public void Advance(bool fixedUpdate, InputState? state)
        {
            if (Plugin.state == FrameState.Continous) return;
            Plugin.state = fixedUpdate ? FrameState.UpdateBothStep : FrameState.UpdateOnlyStep;
            Time.timeScale = timeScaleRef(MonoSingleton<TimeController>.Instance) * timeScaleModifierRef(MonoSingleton<TimeController>.Instance);
            if (state != null) input.queueState(state.Value.keyboard, state.Value.mouse);
        }

        private void PatchAssembly(Harmony harmony)
        {
            var gameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == "Assembly-CSharp");
            foreach (var type in gameAssembly.GetTypes())
            {
                if (!typeof(MonoBehaviour).IsAssignableFrom(type)) continue;
                var updateMethod = type.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var fixedUpdateMethod = type.GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var lateUpdateMethod = type.GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (updateMethod != null) harmony.Patch(updateMethod, prefix: new HarmonyMethod(typeof(FrameController), nameof(UpdateGate)));
                if (fixedUpdateMethod != null) harmony.Patch(fixedUpdateMethod, prefix: new HarmonyMethod(typeof(FrameController), nameof(FixedUpdateGate)));
                if (lateUpdateMethod != null) harmony.Patch(lateUpdateMethod, prefix: new HarmonyMethod(typeof(FrameController), nameof(UpdateGate)));
            }
        }

        public static bool UpdateGate() => Plugin.state != FrameState.Suspended;
        public static bool FixedUpdateGate() => Plugin.state == FrameState.Continous || Plugin.state == FrameState.UpdateBothStep;
    }

    [HarmonyPatch(typeof(Time), nameof(Time.time), MethodType.Getter)]
    static class TimePatch
    {
        static bool Prefix(ref float __result)
        {
            if (Plugin.state == FrameState.Continous) return true;
            __result = FrameController.logicalTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Time), nameof(Time.deltaTime), MethodType.Getter)]
    static class DeltaTimePatch
    {
        static bool Prefix(ref float __result)
        {
            if (Plugin.state == FrameState.Continous) return true;
            if (Plugin.state == FrameState.UpdateBothStep) __result = FrameController.logicalDeltaTime;
            else __result = 0;
            return false;
        }
    }
}