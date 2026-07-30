using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace FreezeFrame
{
    class AsyncQueue<T>
    {
        private readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim signal = new SemaphoreSlim(0);

        public void Enqueue(T item)
        {
            queue.Enqueue(item);
            signal.Release();
        }

        public async Task<T> DequeueAsync(CancellationToken ct = default)
        {
            await signal.WaitAsync(ct);
            queue.TryDequeue(out T item);
            return item;
        }

        public bool TryDequeue(out T item) => queue.TryDequeue(out item);
    }

    public static class StreamExtensions
    {
        public static async Task ReadExactAsync(this Stream stream, byte[] buffer, int offset, int count, CancellationToken ct)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = await stream.ReadAsync(buffer, offset + totalRead, count - totalRead, ct);
                if (read == 0)
                    throw new EndOfStreamException();
                totalRead += read;
            }
        }
    }
}
