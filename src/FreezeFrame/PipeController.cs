using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Collections.Concurrent;

namespace FreezeFrame {
    class PipeController {
        private const string PipeName = "FreezeFrameTAS";
        private ConcurrentQueue<string> rcvd;
        private ConcurrentQueue<string> send;
        private NamedPipeServerStream stream;

        public PipeController() {
            rcvd = new ConcurrentQueue<string>();
            send = new ConcurrentQueue<string>();
            Connect();
        }

        public string Read() {
            if(rcvd.TryDequeue(out string ret)) return ret;
            return null;
        }

        public void Write(string msg) {
            send.Enqueue(msg);
        }

        private async void Connect() {
            stream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            await stream.WaitForConnectionAsync();
            var readThread = new Thread(ReadProc) { IsBackground = true };
            var writeThread = new Thread(WriteProc) { IsBackground = true };
            readThread.Start();
            writeThread.Start();
        }

        private async void Reconnect() {
            return;
        }

        private void ReadProc() {
            using (StreamReader reader = new StreamReader(stream)) {
                while (true) {
                    string msg = reader.ReadLine();
                    if (msg != null) rcvd.Enqueue(reader.ReadLine());
                }
            }
        }

        private void WriteProc() {
            using (StreamWriter writer = new StreamWriter(stream)) {
                while (true) {
                    if (send.TryDequeue(out string msg)) {
                        writer.WriteLine(msg);
                        writer.Flush();
                    }
                }
            }
        }
    }
}