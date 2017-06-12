using System;
using System.Collections.Generic;
using System.Threading;

namespace Rainbow.MessageQueue.Ring
{
    public class MultiSequencer : Sequencer
    {

        //队列使用情况标记，每过一轮+1
        private readonly int[] _availableBuffer;
        //队列最大可存储值下标
        private readonly int _indexMask;
        //位移数量值，如1移1位，2移2位，4移3位，8移4位以此类推
        private readonly int _indexShift;
        //消费者最小消费的序列缓存值
        private Sequence _sequenceCache = new Sequence();

        public MultiSequencer(int bufferSize, IWaitStrategy waitStrategy)
            : base(bufferSize, waitStrategy)
        {
            this._availableBuffer = new int[bufferSize];
            this._indexMask = bufferSize - 1;
            this._indexShift = Util.Log2(bufferSize);
            InitialiseAvailableBuffer();
        }

        #region 私有方法


        private void InitialiseAvailableBuffer()
        {
            for (int i = _availableBuffer.Length - 1; i != 0; i--)
            {
                SetAvailableBufferValue(i, -1);
            }

            SetAvailableBufferValue(0, -1);

        }


        private int CalculateIndex(long sequence)
        {
            return ((int)sequence) & _indexMask;
        }

        private int CalculateAvailabilityFlag(long sequence)
        {
            return (int)((ulong)sequence >> _indexShift);
        }

        private void SetAvailableBufferValue(int index, int flag)
        {
            _availableBuffer[index] = flag;
        }

        private void SetAvailable(long sequence)
        {
            SetAvailableBufferValue(CalculateIndex(sequence), CalculateAvailabilityFlag(sequence));
        }

        private bool IsAvailable(long sequence)
        {
            int index = CalculateIndex(sequence);
            int flag = CalculateAvailabilityFlag(sequence);
            return Volatile.Read(ref _availableBuffer[index]) == flag;
        }
        #endregion

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

            long current;
            long next;

            var spinWait = new SpinWait();
            do
            {
                current = _sequence.Value;
                next = current + n;

                long offsetSequence = next - _bufferSize;
                long cachedGatingSequence = _sequenceCache.Value;

                //获取队列长度与当前处理值比较，如果没有超过，为可用
                if (offsetSequence > cachedGatingSequence || cachedGatingSequence > current)
                {
                    //询问消费者已经处理的最小的序列是多少，并进行设置
                    long gatingSequence = Util.GetMinimum(this._gatingSequences, current);

                    if (offsetSequence > gatingSequence)
                    {
                        spinWait.SpinOnce();
                        continue;
                    }

                    _sequenceCache.SetValue(gatingSequence);
                }
                else if (_sequence.CompareAndSet(current, next))
                {
                    break;
                }
            }
            while (true);

            return next;
        }

        public override void Publish(long sequence)
        {
            SetAvailable(sequence);
        }

        public override void Publish(long lo, long hi)
        {
            for (long l = lo; l <= hi; l++)
            {
                SetAvailable(l);
            }
        }

        public override long GetAvailableSequence(long lo, long hi)
        {
            for (long sequence = lo; sequence <= hi; sequence++)
            {
                if (!IsAvailable(sequence))
                {
                    return sequence - 1;
                }
            }

            return hi;
        }

    }
}