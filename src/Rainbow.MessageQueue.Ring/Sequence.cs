using System.Runtime.InteropServices;
using System.Threading;

namespace Rainbow.MessageQueue.Ring
{
    public class Sequence: ISequence
    {
        public const long InitialCursorValue = -1;
        private Fields _fields;

        public Sequence(long initialValue = InitialCursorValue)
        {
            _fields = new Fields(initialValue);
        }

        public long Value => _fields.Value;

        public bool CompareAndSet(long expectedSequence, long nextSequence)
        {
            return Interlocked.CompareExchange(ref _fields.Value, nextSequence, expectedSequence) == expectedSequence;
        }

        public void SetValue(long value)
        {
            _fields.Value = value;
        }

        [StructLayout(LayoutKind.Explicit, Size = 120)]
        private struct Fields
        {
            //左右偏移一个缓存行，不被其他缓存命中
            //56 = 32 + 24=(32-long(8))
            [FieldOffset(56)]
            public long Value;

            public Fields(long value)
            {
                Value = value;
            }
        }
    }
}