using BepInEx;
using UnityEngine;

namespace FreezeFrame {
    [DefaultExecutionOrder(-10000)]
    [BepInPlugin("com.deweydot.freezeframe", "FreezeFrame", "0.0.1")]
    public class Plugin : BaseUnityPlugin {
        private PipeController pipe;
        private int fc;

        private void Awake() {
            pipe = new PipeController();
            fc = 0;
        }
        
        private void Update() {
            string msg = pipe.Read();
            if (msg != null) HandleMessage(msg);
            if (fc == 0) pipe.Write("ping!");
            fc = (fc + 1) % 500;
        }

        private void HandleMessage(string msg) {
            string[] parts = msg.Split(' ');
            if (parts.Length >= 2 && parts[0] == "ts") {
                if (float.TryParse(parts[1], out float val)) {
                    Time.timeScale = val;
                }
            }
        }
    }
}