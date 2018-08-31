using System;
using System.Collections.Generic;

namespace Rainbow.MessageQueue.Ring
{
    public abstract class AbstractBatchMessageHandler<TMessage> :
        IBatchMessageHandler<TMessage>
        , IMessageHandler<TMessage>
    {
        private List<TMessage> _temps = new List<TMessage>();

        public abstract void Handle(TMessage[] messages, long endSequence);

        public void Handle(TMessage message, long sequence, bool endOfBatch)
        {
            if (!endOfBatch) _temps.Add(message);
            this.Handle(_temps.ToArray(), sequence);
            _temps.Clear();
        }
    }
}