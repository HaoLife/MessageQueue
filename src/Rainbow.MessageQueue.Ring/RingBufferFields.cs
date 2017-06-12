using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Rainbow.MessageQueue.Ring
{

    [StructLayout(LayoutKind.Explicit, Size = 136)]
    public struct RingBufferFields
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
}
