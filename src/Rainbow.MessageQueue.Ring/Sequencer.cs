using System;
using System.Collections.Generic;

namespace Rainbow.MessageQueue.Ring
{
    public abstract class Sequencer : ISequencer
    {
        //队列大小
        protected readonly int _bufferSize;
        protected readonly IWaitStrategy _waitStrategy;
        //当前生产者生产的序列值
        protected Sequence _sequence = new Sequence();
        //序列闸门，用来限制可进行生产的序列
        protected List<ISequence> _gatingSequences;


        public Sequencer(int bufferSize, IWaitStrategy waitStrategy)
        {
            if (bufferSize < 1)
            {
                throw new ArgumentException("bufferSize must not be less than 1");
            }
            if (Util.CeilingNextPowerOfTwo(bufferSize) != bufferSize)
            {
                throw new ArgumentException("bufferSize must be a power of 2");
            }

            _bufferSize = bufferSize;
            _waitStrategy = waitStrategy;
            _gatingSequences = new List<ISequence>();
        }

        public long Current => _sequence.Value;

        public int BufferSize => _bufferSize;


        public abstract long Next();

        public abstract long Next(int n);

        public abstract void Publish(long sequence);
        public abstract void Publish(long lo, long hi);

        public abstract long GetAvailableSequence(long lo, long hi);

        public abstract bool IsUsed(long sequence);

        public void AddGatingSequences(params ISequence[] gatingSequences)
        {
            _gatingSequences.AddRange(gatingSequences);
        }


        public ISequenceBarrier NewBarrier(params ISequence[] sequencesToTrack)
        {
            return new SequenceBarrier(this, _waitStrategy, _sequence, sequencesToTrack);
        }
    }
}