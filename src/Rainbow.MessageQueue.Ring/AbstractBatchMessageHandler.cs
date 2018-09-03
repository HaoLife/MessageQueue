using System;
using System.Collections.Generic;

namespace Rainbow.MessageQueue.Ring
{
    public abstract class AbstractBatchMessageHandler<TMessage> :
        IBatchMessageHandler<TMessage>
        , IMessageHandler<TMessage>
    {
        private static int defaultMaxCache = 5000;

        private List<TMessage> _temps;
        private int maxCache;
        public AbstractBatchMessageHandler()
            : this(defaultMaxCache)
        {

        }
        public AbstractBatchMessageHandler(int maxCache)
        {
            this.maxCache = maxCache;
            this._temps = new List<TMessage>(this.maxCache);
        }


        public abstract void Handle(TMessage[] messages, long endSequence);

        public void Handle(TMessage message, long sequence, bool endOfBatch)
        {
            _temps.Add(message);
            if (endOfBatch || _temps.Count >= maxCache)
            {
                this.Handle(_temps.ToArray(), sequence);
                _temps.Clear();
            }
        }
    }
}