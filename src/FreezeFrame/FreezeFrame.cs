using BepInEx;
using UnityEngine;

namespace FreezeFrame {
    [DefaultExecutionOrder(-10000)]
    [BepInPlugin("com.deweydot.freezeframe", "FreezeFrame", "0.0.1")]
    public class Plugin : BaseUnityPlugin {
        private ManagedPipeServer pipe;

        private void Awake() {
            pipe = new ManagedPipeServer();
            pipe.Start();
        }
        
        private void Update() {
            string msg = pipe.Read();
            if (msg != null) pipe.Write(msg);
        }
    }
}