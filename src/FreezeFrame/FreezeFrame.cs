using System;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace FreezeFrame {
    [BepInPlugin("com.deweydot.freezeframe", "FreezeFrame", "0.0.1")]
    public class Plugin : BaseUnityPlugin {
        private NamedPipeServerStream pipeStream;
        private StreamReader pipeReader;
        private StreamWriter pipeWriter;
        private bool isActive = false;

        private void Awake() {
            ConnectToPipe();
            var pipeThread = new Thread(PipeProc) { IsBackground = true };
            pipeThread.Start();
            Logger.LogInfo("FreezeFrame plugin started.");
        }
        
        private void Update () {
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame) {
                FreezeGame();
            }
        }

        private void ConnectToPipe(){
            pipeStream = new NamedPipeServerStream("FreezeFrameTAS", PipeDirection.InOut);
            pipeStream.WaitForConnection();
            pipeReader = new StreamReader(pipeStream);
            pipeWriter = new StreamWriter(pipeStream);
        }

        private void PipeProc() {
            while (true) {
                string line = pipeReader.ReadLine();
                if (line != null && line == "play") {
                    UnfreezeGame();
                }
            }
        }

        private void FreezeGame() {
            if (isActive) return;
            PlayerLoopSystem currLoop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < currLoop.subSystemList.Length; i++) {
                if (currLoop.subSystemList[i].type == typeof(Update)) {
                    currLoop.subSystemList[i].subSystemList = null;
                }
                else if (currLoop.subSystemList[i].type == typeof(FixedUpdate)) {
                    currLoop.subSystemList[i].subSystemList = null;
                }
            }
            PlayerLoop.SetPlayerLoop(currLoop);
            isActive = true;
        }
        
        private void UnfreezeGame() {
            if (!isActive) return;
            PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
            isActive = false;
        }
    }
}