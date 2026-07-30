using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace FreezeFrame
{
    public class PipeServer
    {
        private const string PipeName = "FreezeFrameTAS";
        private const int BufferSize = 1024;
        private ConcurrentQueue<byte[]> rcvd;
        private AsyncQueue<byte[]> send;
        private byte[] buf;

        public PipeServer()
        {
            rcvd = new ConcurrentQueue<byte[]>();
            send = new AsyncQueue<byte[]>();
            buf = new byte[BufferSize];
        }

        // Fetch a message and if none are present return null
        public byte[] Read()
        {
            if (rcvd.TryDequeue(out byte[] ret)) return ret;
            return null;
        }

        // Send a message
        public void Write(byte[] msg)
        {
            send.Enqueue(msg);
        }

        public async void Start(bool reconnect = true)
        {
            do
            {
                await Connect();
            } while (reconnect);
        }

        private async Task Connect()
        {
            using (var stream = new NamedPipeServerStream(PipeName, PipeDirection.InOut,
                1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                await stream.WaitForConnectionAsync(); // wait for client
                using (var cts = new CancellationTokenSource())
                { // start queue consumers
                    var readTask = ReadProc(stream, cts.Token);
                    var writeTask = WriteProc(stream, cts.Token);
                    await Task.WhenAny(readTask, writeTask); // await connection end
                    cts.Cancel(); // signal tasks to end
                    await WaitForProc(readTask); // await task cleanup
                    await WaitForProc(writeTask);
                    stream.Dispose();
                }
            }
        }

        private async Task ReadProc(NamedPipeServerStream stream, CancellationToken ct)
        {
            int size;
            while (true)
            {
                await stream.ReadExactAsync(buf, 0, 1, ct); // get first byte
                if (buf[0] < 128)
                { // payloadless messages
                    rcvd.Enqueue(buf[..1]);
                    continue;
                }
                await stream.ReadExactAsync(buf, 1, 3, ct); // get header length
                size = ToUInt24(buf, 1);
                if (size + 4 <= BufferSize)
                { // if fits within buf
                    await stream.ReadExactAsync(buf, 4, size, ct);
                    rcvd.Enqueue(buf[..(size + 4)]);
                }
                else
                { // create new buffer
                    byte[] tmpBuf = new byte[size + 4];
                    Array.Copy(buf, 0, tmpBuf, 0, 4);
                    await stream.ReadExactAsync(tmpBuf, 4, size, ct);
                    rcvd.Enqueue(tmpBuf);
                }
            }
        }

        private async Task WriteProc(NamedPipeServerStream stream, CancellationToken ct)
        {
            while (true)
            {
                byte[] msg = await send.DequeueAsync(ct);
                await stream.WriteAsync(msg, ct);
                await stream.FlushAsync(ct);
            }
        }

        private async Task WaitForProc(Task t)
        {
            try { await t; }
            catch (ObjectDisposedException) { } // discard expected exceptions
            catch (IOException) { }
            catch (OperationCanceledException) { }
        }

        private static int ToUInt24(byte[] buffer, int offset)
        {
            return buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16);
        }
    }
}