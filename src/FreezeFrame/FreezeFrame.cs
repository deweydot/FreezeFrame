using System;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using BepInEx;

namespace FreezeFrame {
    [BepInPlugin("com.deweydot.freezeframe", "FreezeFrame", "0.0.1")]
    public class Plugin : BaseUnityPlugin {
        private NamedPipeServerStream pipeStream;
        private StreamReader pipeReader;
        private StreamWriter pipeWriter;

        private void Awake() {
            ConnectToPipe();
            var pipeThread = new Thread(PipeProc) { IsBackground = true };
            pipeThread.Start();
            Logger.LogInfo("FreezeFrame plugin started.");
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
                if (line != null) {
                    Logger.LogInfo(line);
                }
                pipeWriter.WriteLine("rcvd");
                pipeWriter.Flush();
            }
        }
    }
}