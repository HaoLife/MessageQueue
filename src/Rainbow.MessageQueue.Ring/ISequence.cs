namespace Rainbow.MessageQueue.Ring
{
    public interface ISequence
    {
        long Value { get; }
        void SetValue(long value);
        bool CompareAndSet(long expectedSequence, long nextSequence);
    }
}