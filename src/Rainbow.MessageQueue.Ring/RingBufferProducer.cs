using System.Collections.Generic;
using System.Linq;

namespace Rainbow.MessageQueue.Ring
{
    public class RingBufferProducer<TMessage> : IRingBufferProducer<TMessage>
    {
        private readonly IRingBuffer<TMessage> _ringQueue;
        public RingBufferProducer(IRingBuffer<TMessage> ringQueue)
        {
            this._ringQueue = ringQueue;
        }
        
        public long Send(IEnumerable<TMessage> messages)
        {
            var size = messages.Count();
            var events = messages.ToArray();
            var hi = this._ringQueue.Next(size);
            var lo = hi - (size - 1);
            for (var l = lo; l <= hi; l++)
            {
                this._ringQueue[l].Value = events[l - lo];
            }
            this._ringQueue.Publish(lo, hi);
            return hi;
        }

        public long Send(TMessage message)
        {
            var sequence = this._ringQueue.Next();
            this._ringQueue[sequence].Value = message;
            this._ringQueue.Publish(sequence);
            return sequence;
        }
    }
}