using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FreezeFrame
{
    class FrameController
    {
        public static FrameState state = FrameState.Continuous;
        public static float logicalTime = 0f;
        public const float logicalDeltaTime = 0.008f; // 1/125 seconds
        private AccessTools.FieldRef<TimeController, float> timeScaleRef;
        private AccessTools.FieldRef<TimeController, float> timeScaleModifierRef;

        public FrameController(Harmony harmony)
        {
            PatchAssembly(harmony);
            timeScaleRef = AccessTools.FieldRefAccess<TimeController, float>("timeScale");
            timeScaleModifierRef = AccessTools.FieldRefAccess<TimeController, float>("timeScaleModifier");
        }

        public void Update()
        {
            if (state == FrameState.UpdateOnlyStep ||
                state == FrameState.UpdateBothStep)
            {
                Enable();
            }
        }

        public void LateUpdate()
        {
            if (state == FrameState.UpdateBothStep)
            {
                Physics.Simulate(0.008f);
                logicalTime += logicalDeltaTime;
            }
        }

        public void Enable()
        {
            state = FrameState.Suspended;
            Time.captureDeltaTime = 0.008f;
            Physics.simulationMode = SimulationMode.Script;
            Time.timeScale = 0f;
        }

        public void Disable()
        {
            state = FrameState.Continuous;
            Time.captureDeltaTime = 0f;
            Physics.simulationMode = SimulationMode.FixedUpdate;
            Time.timeScale = timeScaleRef(MonoSingleton<TimeController>.Instance) * timeScaleModifierRef(MonoSingleton<TimeController>.Instance);
        }

        public void Advance(bool fixedUpdate)
        {
            if (state == FrameState.Continuous) return;
            state = fixedUpdate ? FrameState.UpdateBothStep : FrameState.UpdateOnlyStep;
            Time.timeScale = timeScaleRef(MonoSingleton<TimeController>.Instance) * timeScaleModifierRef(MonoSingleton<TimeController>.Instance);
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

        public static bool UpdateGate() => state != FrameState.Suspended;
        public static bool FixedUpdateGate() => state == FrameState.Continuous || state == FrameState.UpdateBothStep;
    }

    public enum FrameState
    {
        Suspended,
        UpdateOnlyStep,
        UpdateBothStep,
        Continuous
    }
}