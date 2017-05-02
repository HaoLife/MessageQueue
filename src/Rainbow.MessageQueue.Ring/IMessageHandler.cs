namespace Rainbow.MessageQueue.Ring
{
    public interface IMessageHandler<TMessage>
    {
        void Handle(TMessage[] messages);
    }
}