namespace Rainbow.MessageQueue.Ring
{
    public interface ISequenceBarrier
    {
        long WaitFor(long sequence);

        long Cursor { get; }
    }
}