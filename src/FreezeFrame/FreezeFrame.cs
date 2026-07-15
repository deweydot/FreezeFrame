using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading;

using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FreezeFrame {
    [DefaultExecutionOrder(-10000)]
    [BepInPlugin("com.deweydot.freezeframe", "FreezeFrame", "0.0.1")]
    public class FreezeFrame : BaseUnityPlugin {
        private NamedPipeServerStream pipeStream;
        private StreamReader pipeReader;
        private StreamWriter pipeWriter;
        private static int stepFrames = -1;
        public static float logicalTime = 0f;
        public static float logicalDeltaTime = 0.008f;

        private void Awake() {
            var harmony = new Harmony("com.deweydot.freezeframe");
            PatchAssembly(harmony);
            harmony.PatchAll();
            //ConnectToPipe();
            Logger.LogInfo("FreezeFrame plugin started.");
        }
        
        private void Update() {
            if (stepFrames > 0){
                stepFrames--;
            }
            if (Keyboard.current != null) {
                // f1 to pause, f2 to step, f3 to resume
                if (Keyboard.current.f1Key.wasPressedThisFrame && !isActive()) {
                    stepFrames = 0;
                    Time.captureDeltaTime = 0.008f;
                    Physics.simulationMode = SimulationMode.Script;
                }
                else if (Keyboard.current.f2Key.wasPressedThisFrame && isActive()) {
                    stepFrames = 1;
                }
                else if (Keyboard.current.f3Key.wasPressedThisFrame && isActive()) {
                    stepFrames = -1;
                    Time.captureDeltaTime = 0f;
                    Physics.simulationMode = SimulationMode.FixedUpdate;
                }
            }
        }

        private void LateUpdate() {
            if (stepFrames > 0){
                Physics.Simulate(0.008f);
                logicalTime += logicalDeltaTime;
            }
        }

        private void PatchAssembly(Harmony harmony) {
            var gameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(a => a.GetName().Name == "Assembly-CSharp");
            foreach (var type in gameAssembly.GetTypes()) {
                if (!typeof(MonoBehaviour).IsAssignableFrom(type)) continue;
                foreach (var methodName in new[] { "Update", "FixedUpdate", "LateUpdate" }) {
                    var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method == null) continue;
                    harmony.Patch(method, prefix: new HarmonyMethod(typeof(FreezeFrame), nameof(FreezePatch)));
                }
            }
        }

        static bool FreezePatch() => stepFrames != 0;

        public static bool isActive() => stepFrames >= 0;

        private void ConnectToPipe() {
            pipeStream = new NamedPipeServerStream("FreezeFrameTAS", PipeDirection.InOut);
            pipeStream.WaitForConnection();
            pipeReader = new StreamReader(pipeStream);
            pipeWriter = new StreamWriter(pipeStream);
            var pipeThread = new Thread(PipeProc) { IsBackground = true };
            pipeThread.Start();
        }

        private void PipeProc() {
            while (true) {
                string line = pipeReader.ReadLine();
                if (line != null) {
                    continue;
                }
            }
        }
    }
}