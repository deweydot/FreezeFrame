using System;
using System.Linq;
using System.Reflection;

using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FreezeFrame {
    [DefaultExecutionOrder(-10000)]
    [BepInPlugin("com.deweydot.freezeframe", "FreezeFrame", "0.0.1")]
    public class FreezeFrame : BaseUnityPlugin {
        private static int stepFrames = -1;
        public static float logicalTime = 0f;
        public static float logicalDeltaTime = 0.008f;

        private void Awake() {
            var harmony = new Harmony("com.deweydot.freezeframe");
            PatchAssembly(harmony);
            harmony.PatchAll();
            Logger.LogInfo("FreezeFrame plugin started.");
        }
        
        private void Update() {
            if (stepFrames > 0){
                stepFrames--;
                Time.timeScale = 0f;
            }
            if (Keyboard.current != null) {
                // f1 to pause, f2 to step, f3 to resume
                if (Keyboard.current.f1Key.wasPressedThisFrame && !IsActive()) {
                    EnableFreeze();
                }
                else if (Keyboard.current.f2Key.wasPressedThisFrame && IsActive()) {
                    stepFrames = 1;
                    Time.timeScale = 1f;
                }
                else if (Keyboard.current.f3Key.wasPressedThisFrame && IsActive()) {
                    DisableFreeze();
                }
            }
        }

        private void LateUpdate() {
            if (stepFrames > 0){
                Physics.Simulate(0.008f);
                logicalTime += logicalDeltaTime;
            }
        }

        private void EnableFreeze() {
            stepFrames = 0;
            Time.captureDeltaTime = 0.008f;
            Physics.simulationMode = SimulationMode.Script;
            Time.timeScale = 0f;
        }

        private void DisableFreeze() {
            stepFrames = -1;
            Time.captureDeltaTime = 0f;
            Physics.simulationMode = SimulationMode.FixedUpdate;
            Time.timeScale = 1f;
        }

        private void PatchAssembly(Harmony harmony) {
            var gameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == "Assembly-CSharp");
            foreach (var type in gameAssembly.GetTypes()) {
                if (!typeof(MonoBehaviour).IsAssignableFrom(type)) continue;
                foreach (var methodName in new[] { "Update", "FixedUpdate", "LateUpdate" }) {
                    var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method == null) continue;
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(FreezeFrame), nameof(FreezeGate)));
                }
            }
        }

        static bool FreezeGate() => stepFrames != 0;

        public static bool IsActive() => stepFrames >= 0;
    }
}