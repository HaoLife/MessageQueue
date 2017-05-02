namespace Rainbow.MessageQueue.Ring
{
    public interface IQueueConsumer
    {
         
        Sequence Sequence { get; }

        void Run();
    }
}