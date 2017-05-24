using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Rainbow.MessageQueue.Ring
{
    public class SingleSequencer : Sequencer
    {

        [StructLayout(LayoutKind.Explicit, Size = 128)]
        private struct Fields
        {
            [FieldOffset(56)]
            public long NextValue;
            [FieldOffset(64)]
            public long CachedValue;

            public Fields(long nextValue, long cachedValue)
            {
                NextValue = nextValue;
                CachedValue = cachedValue;
            }
        }


        private Fields _fields = new Fields(Sequence.InitialCursorValue, Sequence.InitialCursorValue);

        public SingleSequencer(int bufferSize, IWaitStrategy waitStrategy)
            : base(bufferSize, waitStrategy)
        {

        }

        public override long GetAvailableSequence(long lo, long hi)
        {
            return hi;
        }


        public override bool IsUsed(long sequence)
        {
            return sequence <= _fields.NextValue;
        }

        public override long Next()
        {
            return Next(1);
        }

        public override long Next(int n)
        {
            if (n < 1)
            {
                throw new ArgumentException("n must be > 0");
            }

            long nextValue = _fields.NextValue;

            long nextSequence = nextValue + n;
            long offsetSequence = nextSequence - _bufferSize;
            long cachedGatingSequence = _fields.CachedValue;

            if (offsetSequence > cachedGatingSequence || cachedGatingSequence > nextValue)
            {
                _sequence.SetValue(nextSequence);

                var spinWait = default(SpinWait);
                long minSequence;
                while (offsetSequence > (minSequence = Util.GetMinimum(Volatile.Read(ref _gatingSequences), nextValue)))
                {
                    spinWait.SpinOnce();
                }

                _fields.CachedValue = minSequence;
            }

            _fields.NextValue = nextSequence;

            return nextSequence;
        }

        public override void Publish(long sequence)
        {
            _sequence.SetValue(sequence);
        }

        public override void Publish(long lo, long hi)
        {
            Publish(hi);
        }
    }
}