using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO.Pipes;
using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FreezeFrame {
    [BepInPlugin("com.deweydot.freezeframe", "FreezeFrame", "0.0.1")]
    public class Plugin : BaseUnityPlugin {
        private static readonly ConcurrentQueue<string> CommandQueue = new ConcurrentQueue<string>();
        private NamedPipeServerStream pipeStream;

        private void Awake() {
            var pipeThread = new Thread(PipeLoop) { IsBackground = true };
            pipeThread.Start();
            Logger.LogInfo("FreezeFrame plugin started.");
        }

        private void Update() {
            // TODO
        }

        private void ConnectToPipe(){
            var pipe = new NamedPipeServerStream("FreezeFrameTAS", PipeDirection.InOut);
            pipe.WaitForConnection();
            return pipe;
        }

        private void PipeProc() {
            while (true) {
                string line = Console.ReadLine();
                if (line != null)
                    Logger.LogInfo(line);
            }
        }
    }
}