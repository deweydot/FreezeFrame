using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;

namespace FreezeFrame {
    class AsyncQueue<T> {
        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim signal = new SemaphoreSlim(0);

        public void Enqueue(T item) {
            queue.Enqueue(item);
            signal.Release();
        }
        
        public async Task<T> DequeueAsync(CancellationToken ct = default) {
            await signal.WaitAsync(ct);
            queue.TryDequeue(out T item);
            return item;
        }

        public bool TryDequeue(out T item) => queue.TryDequeue(out item);
    }

    class ManagedPipeServer {
        private const string PipeName = "FreezeFrameTAS";
        private ConcurrentQueue<string> rcvd;
        private AsyncQueue<string> send;

        public ManagedPipeServer() {
            rcvd = new ConcurrentQueue<string>();
            send = new AsyncQueue<string>();
        }

        public string Read() {
            if(rcvd.TryDequeue(out string ret)) return ret;
            return null;
        }

        public void Write(string msg) {
            send.Enqueue(msg);
        }

        public async void Start() {
            while (true) {
                await Connect();
            }
        }

        private async Task Connect() {
            using (var stream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 
                1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous)) {
                await stream.WaitForConnectionAsync(); // wait for client
                using (var cts = new CancellationTokenSource()) { // start queue consumers
                    var readTask = ReadProc(stream);
                    var writeTask = WriteProc(stream, cts.Token);
                    await Task.WhenAny(readTask, writeTask); // await connection end
                    cts.Cancel(); // clean up both tasks
                    stream.Dispose();
                    await WaitForProc(readTask);
                    await WaitForProc(writeTask);
                }
            }
        }

        private async Task ReadProc(NamedPipeServerStream stream) {
            using (StreamReader reader = new StreamReader(stream)) {
                while (true) {
                    string msg = await reader.ReadLineAsync();
                    if (msg != null) rcvd.Enqueue(msg);
                    else break;
                }
            }
        }
        
        private async Task WriteProc(NamedPipeServerStream stream, CancellationToken ct) {
            using (StreamWriter writer = new StreamWriter(stream)) {
                while (true) {
                    string msg = await send.DequeueAsync(ct);
                    await writer.WriteLineAsync(msg);
                    await writer.FlushAsync();
                }
            }
        }

        private async Task WaitForProc(Task t) {
            try { await t; }
            catch (ObjectDisposedException) { }
            catch (IOException) { }
            catch (OperationCanceledException) { }
        }
    }
}