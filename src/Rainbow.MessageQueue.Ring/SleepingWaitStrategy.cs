using System;
using System.Threading;

namespace Rainbow.MessageQueue.Ring
{
    public class SleepingWaitStrategy : IWaitStrategy
    {
        public long WaitFor(long sequence, ISequence cursor, ISequence dependentSequence, ISequenceBarrier barrier)
        {
            long availableSequence;

            while ((availableSequence = dependentSequence.Value) < sequence)
            {
                barrier.CheckAlert();
                Thread.Sleep(0);
            }

            return availableSequence;
        }
    }
}