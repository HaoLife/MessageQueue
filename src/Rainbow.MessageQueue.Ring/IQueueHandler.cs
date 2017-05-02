namespace Rainbow.MessageQueue.Ring
{
    public interface IQueueHandler<TMessage>
    {
        void Handle(TMessage message, long sequence, bool isEnd);
    }
}