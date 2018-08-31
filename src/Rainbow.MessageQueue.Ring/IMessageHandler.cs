namespace Rainbow.MessageQueue.Ring
{
    public interface IMessageHandler<TMessage>
    {
        void Handle(TMessage message, long sequence, bool endOfBatch);
    }
}