using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace FreezeFrame {
    [DefaultExecutionOrder(-10000)]
    [BepInPlugin("com.deweydot.freezeframe", "FreezeFrame", "0.0.1")]
    public class FreezeFrame : BaseUnityPlugin {
        public static FrameState state = FrameState.Continous;
        private ManagedPipeServer pipe;
        
        private void Awake() {
            Application.runInBackground = true;
            var harmony = new Harmony("com.deweydot.freezeframe");
            FrameController.Init(harmony);
            pipe = new ManagedPipeServer();
            pipe.Start();
        }
        
        private void Update() {
            FrameController.Update();
            this.HandleMessage(pipe);
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