using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using static FreezeFrame.Protocol;

namespace FreezeFrame
{
    class PipeServer
    {
        private ConcurrentQueue<Message> rcvd;
        private AsyncQueue<Message> send;

        public PipeServer()
        {
            rcvd = new ConcurrentQueue<Message>();
            send = new AsyncQueue<Message>();
        }

        // Fetch a message and if none are present return null
        public Message? Read()
        {
            if (rcvd.TryDequeue(out Message ret)) return ret;
            return null;
        }

        // Send a message
        public void Write(Message msg)
        {
            if (msg.payload != null && msg.payload.Length > MaxPayloadLength) throw new InvalidDataException("Invalid Payload Length");
            send.Enqueue(msg);
        }

        public async void RunAsync(CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                try { await Connect(ct); }
                catch (OperationCanceledException) { break; }
                catch (Exception) {  }
            }
        }

        private async Task Connect(CancellationToken ct = default)
        {
            using (var stream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly))
            {
                await stream.WaitForConnectionAsync(ct); // wait for client
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    var readTask = ReadProc(stream, cts.Token); // start read write procs
                    var writeTask = WriteProc(stream, cts.Token);
                    await Task.WhenAny(readTask, writeTask); // await connection end
                    cts.Cancel(); // signal other task to end
                    await WaitForProc(Task.WhenAll(readTask)); // await task cleanup
                }
            }
        }

        private async Task ReadProc(NamedPipeServerStream stream, CancellationToken ct = default)
        {
            int size;
            byte[] header = new byte[4];
            while (!ct.IsCancellationRequested)
            {
                await stream.ReadExactlyAsync(header, 0, 1, ct); // get first byte
                if ((header[0] & PayloadFlag) == 0) // payloadless messages
                {
                    rcvd.Enqueue(new Message { opcode = header[0], payload = null });
                    continue;
                }
                await stream.ReadExactlyAsync(header, 1, HeaderBytes - 1, ct); // get header length
                size = ToUInt24(header, 1);
                byte[] buf = new byte[size];
                await stream.ReadExactlyAsync(buf, 0, size, ct);
                rcvd.Enqueue(new Message { opcode = header[0], payload = buf });
            }
        }

        private async Task WriteProc(NamedPipeServerStream stream, CancellationToken ct = default)
        {
            while (true)
            {
                Message msg = await send.DequeueAsync(ct);
                if (msg.payload == null) await stream.WriteAsync(new byte[] { msg.opcode }, ct);
                else
                {
                    int size = msg.payload.Length;
                    byte[] buf = new byte[4 + size];
                    buf[0] = msg.opcode;
                    buf[1] = (byte)size;
                    buf[2] = (byte)(size >> 8);
                    buf[3] = (byte)(size >> 16);
                    Buffer.BlockCopy(msg.payload, 0, buf, 4, size);
                    await stream.WriteAsync(buf, ct);
                }
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

        public struct Message
        {
            public byte opcode;
            public byte[] payload;
        }
    }
}