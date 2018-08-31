namespace Rainbow.MessageQueue.Ring
{
    public interface IBatchMessageHandler<TMessage>
    {
        void Handle(TMessage[] messages, long endSequence);

    }
}