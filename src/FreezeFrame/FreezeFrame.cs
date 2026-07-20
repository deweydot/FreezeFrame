using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace FreezeFrame {
    [DefaultExecutionOrder(-10000)]
    [BepInPlugin("com.deweydot.freezeframe", "FreezeFrame", "0.0.1")]
    public class FreezeFrame : BaseUnityPlugin {
        private ManagedPipeServer pipe;
        
        private void Awake() {
            Application.runInBackground = true;
            var harmony = new Harmony("com.deweydot.frameadvance");
            FrameController.ApplyPatch(harmony);
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
                    FrameController.Advance(1);
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
}