using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Rainbow.MessageQueue.Ring
{
    public class RingBuffer<TMessage> : IRingBuffer<TMessage>
    {

        #region 内部类成员


        [StructLayout(LayoutKind.Explicit, Size = 135)]
        private struct RingBufferFields
        {
            //易变
            [FieldOffset(56)]
            public object[] Data;

            //不变
            [FieldOffset(64)]
            public int Size;

            [FieldOffset(68)]
            public int IndexMask;

            [FieldOffset(72)]
            public ISequencer Sequencer;

        }
        #endregion

        private RingBufferFields _value;

        public RingBuffer(ISequencer sequencer)
        {
            this._value.Size = sequencer.BufferSize;

            if (this._value.Size < 1)
            {
                throw new ArgumentException("bufferSize must not be less than 1");
            }
            if (Util.CeilingNextPowerOfTwo(this._value.Size) != this._value.Size)
                throw new ArgumentException("bufferSize must be a power of 2");

            this._value.IndexMask = sequencer.BufferSize - 1;
            this._value.Sequencer = sequencer;
            Fill();
        }

        #region 内部方法

        private void Fill()
        {
            this._value.Data = new object[this._value.Size];
            for (int i = 0; i < this._value.Size; i++)
            {
                this._value.Data[i] = new WrapMessage<TMessage>();
            }
        }


        #endregion

        public WrapMessage<TMessage> this[long sequence] => (WrapMessage<TMessage>)_value.Data[(sequence & _value.IndexMask)];

        public int Size => this._value.Size;

        public long Next()
        {
            return this._value.Sequencer.Next();
        }

        public void Publish(long sequence)
        {
            this._value.Sequencer.Publish(sequence);
        }

        public long Next(int n)
        {
            return this._value.Sequencer.Next(n);
        }

        public void Publish(long lo, long hi)
        {
            this._value.Sequencer.Publish(lo, hi);
        }

        public void AddGatingSequences(params ISequence[] gatingSequences)
        {
            this._value.Sequencer.AddGatingSequences(gatingSequences);
        }

        public ISequenceBarrier NewBarrier(params ISequence[] sequencesToTrack)
        {
            return this._value.Sequencer.NewBarrier(sequencesToTrack);
        }
    }
}