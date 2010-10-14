using System.Collections.Generic;
using System.Threading;

namespace WinputDotNet.Providers {
    // Adopted from http://stackoverflow.com/questions/530211/creating-a-blocking-queuet-in-net/530228#530228
    internal class BlockingQueue<T> {
        private readonly Queue<T> queue = new Queue<T>();
        private bool cancelled;

        public void Enqueue(T item) {
            lock (this.queue) {
                this.queue.Enqueue(item);

                if (queue.Count == 1) {
                    // Wake up any blocked dequeue
                    Monitor.PulseAll(this.queue);
                }
            }
        }

        public bool TryDequeue(out T value) {
            lock (this.queue) {
                while (!this.cancelled && this.queue.Count == 0) {
                    // Wait for an enqueue
                    Monitor.Wait(this.queue);
                }

                if (this.cancelled) {
                    value = default(T);
                    return false;
                }

                value = queue.Dequeue();

                return true;
            }
        }

        public void Clear() {
            lock (this.queue) {
                queue.Clear();
            }
        }

        public void Cancel() {
            lock (this.queue) {
                this.cancelled = true;

                Monitor.PulseAll(this.queue);
            }
        }
    }
}