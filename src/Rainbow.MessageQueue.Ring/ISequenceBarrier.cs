namespace Rainbow.MessageQueue.Ring
{
    public interface ISequenceBarrier
    {
        long WaitFor(long sequence);

        long Cursor { get; }

        bool IsAlerted { get; }

        void Alert();

        void ClearAlert();

        void CheckAlert();
    }
}