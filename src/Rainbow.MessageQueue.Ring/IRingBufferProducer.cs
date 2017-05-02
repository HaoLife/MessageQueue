using System.Collections.Generic;

namespace Rainbow.MessageQueue.Ring
{
    public interface IRingBufferProducer<TMessage>
    {
        long Send(TMessage message);
        long Send(IEnumerable<TMessage> messages);
    }
}