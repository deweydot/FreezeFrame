using System.IO.Pipes;
using System.Threading.Channels;

namespace FreezeFrame
{
    class PipeClient
    {
        private Channel<Message> rcvd;
        private Channel<Message> send;

        public PipeClient()
        {
            rcvd = Channel.CreateUnbounded<Message>();
            send = Channel.CreateUnbounded<Message>();
        }

        public async Task RunAsync(CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                try { await Connect(ct); }
                catch (OperationCanceledException) { break; }
                catch (Exception) { }
            }
        }

        private async Task Connect(CancellationToken ct = default)
        {
            using (var stream = new NamedPipeClientStream(".", Protocol.Consts.PipeName, PipeDirection.InOut,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly))
            {
                await stream.ConnectAsync(); // connect to server
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

        private async Task ReadProc(NamedPipeClientStream stream, CancellationToken ct = default)
        {
            int size;
            byte[] header = new byte[4];
            while (!ct.IsCancellationRequested)
            {
                await stream.ReadExactlyAsync(header, 0, 1, ct); // get first byte
                if ((header[0] & Protocol.Consts.PayloadFlag) == 0) // payloadless messages
                {
                    await rcvd.Writer.WriteAsync(new Message { opcode = header[0], payload = null }, ct);
                    continue;
                }
                await stream.ReadExactlyAsync(header, 1, Protocol.Consts.HeaderBytes - 1, ct); // get header length
                size = ToUInt24(header.AsSpan(1, Protocol.Consts.HeaderBytes - 1))
                byte[] buf = new byte[size];
                await stream.ReadExactlyAsync(buf, 0, size, ct);
                await rcvd.Writer.WriteAsync(new Message { opcode = header[0], payload = buf }, ct);
            }
        }

        private async Task WriteProc(NamedPipeClientStream stream, CancellationToken ct = default)
        {
            while (true)
            {
                Message msg = await send.Reader.ReadAsync(ct);
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

        private static int ToUInt24(ReadOnlySpan<byte> buffer)
        {
            return buffer[0] | (buffer[1] << 8) | (buffer[2] << 16);
        }

        public struct Message
        {
            public byte opcode;
            public byte[]? payload;
        }
    }
}