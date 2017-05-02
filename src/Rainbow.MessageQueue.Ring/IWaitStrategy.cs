namespace Rainbow.MessageQueue.Ring
{
    public interface IWaitStrategy
    {
        long WaitFor(long sequence, ISequence cursor, ISequence dependentSequence, ISequenceBarrier barrier);
    }
}