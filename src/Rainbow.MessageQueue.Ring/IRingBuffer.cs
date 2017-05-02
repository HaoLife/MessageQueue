using System.Collections.Generic;

namespace Rainbow.MessageQueue.Ring
{
    public interface IRingBuffer<TMessage>
    {
        WrapMessage<TMessage> this[long sequence] { get; }
        int Size { get; }
        long Next();
        long Next(int n);
        void Publish(long sequence);
        void Publish(long lo, long hi);
        ISequenceBarrier NewBarrier(params ISequence[] sequencesToTrack);
    }
}