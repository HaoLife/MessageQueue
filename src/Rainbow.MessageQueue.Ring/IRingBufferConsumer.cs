namespace Rainbow.MessageQueue.Ring
{
    public interface IRingBufferConsumer
    {
        Sequence Sequence { get; }
        void Run();

        void Halt();

        bool IsRunning { get; }
    }
}