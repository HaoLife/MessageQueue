using System.Threading;

namespace Rainbow.MessageQueue.Ring
{
    public class SpinWaitStrategy: IWaitStrategy
    {
        public long WaitFor(long sequence, ISequence cursor, ISequence dependentSequence, ISequenceBarrier barrier)
        {
            long availableSequence;
            var spinWait = new SpinWait();

            while ((availableSequence = dependentSequence.Value) < sequence)
            {
                barrier.CheckAlert();
                spinWait.SpinOnce();
            }

            return availableSequence;
        }
    }
}