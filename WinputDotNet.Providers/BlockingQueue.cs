using System.Collections.Generic;
using System.Threading;

namespace WinputDotNet.Providers {
    // Adopted from http://stackoverflow.com/questions/530211/creating-a-blocking-queuet-in-net/530228#530228

    // This class should not be changed to allow for it to become
    // "uncancelled"; allowing this would be catastrophic.

    /// <summary>
    /// Represents a blocking queue.  It blocks on dequeue requests, waiting
    /// for something to be enqueued.  This blocking can be cancelled.
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the queue.</typeparam>
    internal class BlockingQueue<T> {
        /// <summary>
        /// The actual queue containing the items.  Serves as the
        /// synchronizing object.
        /// </summary>
        private readonly Queue<T> queue = new Queue<T>();

        /// <summary>
        /// If true, dequeueing no longer gives any items.
        /// </summary>
        private bool isCancelled;

        /// <summary>
        /// Enqueues the specified item.  Wakes up blocked
        /// <see cref="TryDequeue"/> calls.
        /// </summary>
        /// <param name="item">The item to add to the queue.</param>
        public void Enqueue(T item) {
            lock (this.queue) {
                this.queue.Enqueue(item);

                if (queue.Count == 1) {
                    // Wake up any blocked dequeue
                    Monitor.PulseAll(this.queue);
                }
            }
        }

        /// <summary>
        /// Tries to dequeue an item, blocking until an item is enqueued or
        /// the <see cref="BlockingQueue&lt;T&gt;"/> becomes cancelled.
        /// </summary>
        /// <remarks>
        /// If the <see cref="BlockingQueue&lt;T&gt;"/> is cancelled, the value
        /// of <paramref name="value"/> is undefined.
        /// </remarks>
        /// <param name="value">The value which is dequeued.</param>
        /// <returns>
        /// True if an item was dequeued; false if the <see cref="BlockingQueue&lt;T&gt;"/> was cancelled.
        /// </returns>
        public bool TryDequeue(out T value) {
            lock (this.queue) {
                while (!this.isCancelled && this.queue.Count == 0) {
                    // Wait for an enqueue
                    Monitor.Wait(this.queue);
                }

                if (this.isCancelled) {
                    value = default(T);

                    return false;
                }

                value = queue.Dequeue();

                return true;
            }
        }

        /// <summary>
        /// Cancels any active dequeues and prevents further dequeueing.
        /// </summary>
        public void Cancel() {
            lock (this.queue) {
                this.isCancelled = true;

                Monitor.PulseAll(this.queue);
            }
        }
    }
}