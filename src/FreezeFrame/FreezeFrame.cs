using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FreezeFrame {
    [DefaultExecutionOrder(-10000)]
    [BepInPlugin("com.deweydot.freezeframe", "FreezeFrame", "0.0.1")]
    public class FreezeFrame : BaseUnityPlugin {
        public static FrameState state = FrameState.Continous;
        private ManagedPipeServer pipe;
        
        private void Awake() {
            Application.runInBackground = true;
            FrameController.Init(new Harmony("com.deweydot.frameadvance"));
            pipe = new ManagedPipeServer();
            pipe.Start();
        }
        
        private void Update() {
            FrameController.Update();
            string msg = pipe.Read();
            if (msg == null) return;
            switch (msg) {
                case "stop":
                    FrameController.Enable();
                    break;
                case "step":
                    FrameController.Advance(true);
                    break;
                case "play":
                    FrameController.Disable();
                    break;
                default:
                    break;
            }
        }

        private void LateUpdate() {
            FrameController.LateUpdate();
        }
    }
    
    public enum FrameState {
        Suspended,
        UpdateOnlyStep,
        UpdateBothStep,
        Continous
    }
}