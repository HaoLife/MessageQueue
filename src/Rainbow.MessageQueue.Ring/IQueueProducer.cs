using System.Collections.Generic;

namespace Rainbow.MessageQueue.Ring
{
    public interface IQueueProducer<TMessage>
    {
        long Send(TMessage message);
        long Send(IEnumerable<TMessage> messages);
    }
}