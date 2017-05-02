using System;
using System.Threading;

namespace Rainbow.MessageQueue.Ring
{
    public class RingBufferConsumer<TMessage> : IRingBufferConsumer
    {

        private volatile int _running;
        private readonly IRingBuffer<TMessage> _messageBuffer;
        private readonly ISequenceBarrier _sequenceBarrier;
        private readonly IMessageHandler<TMessage> _batchMessageHandler;
        private Sequence _current = new Sequence();
        private readonly int _maxHandleSize;
        private const int _defaultMaxHandleSize = 100;

        public RingBufferConsumer(
            IRingBuffer<TMessage> messageQueue,
            ISequenceBarrier sequenceBarrier,
            IMessageHandler<TMessage> batchMessageHandler,
            int maxHandleSize = _defaultMaxHandleSize)
        {
            if (maxHandleSize < 0) throw new ArgumentException($"{nameof(maxHandleSize)} must greater than 0", nameof(maxHandleSize));

            this._messageBuffer = messageQueue;
            this._sequenceBarrier = sequenceBarrier;
            this._batchMessageHandler = batchMessageHandler;
            this._maxHandleSize = maxHandleSize;
        }

        public Sequence Sequence => _current;

        public bool IsRunning => this._running == 1;

        public void Halt()
        {
            _running = 0;
            _sequenceBarrier.Alert();
        }
        public void Run()
        {
            if (Interlocked.Exchange(ref _running, 1) != 0)
            {
                throw new InvalidOperationException("Thread is already running");
            }

            while (true)
            {
                try
                {
                    var nextSequence = _current.Value + 1L;
                    var availableSequence = _sequenceBarrier.WaitFor(nextSequence);
                    availableSequence = availableSequence > nextSequence + _maxHandleSize ? nextSequence + _maxHandleSize : availableSequence;

                    if (nextSequence <= availableSequence)
                    {
                        TMessage[] messages = new TMessage[availableSequence - nextSequence];
                        var temp = nextSequence;
                        while (nextSequence <= availableSequence)
                        {
                            var evt = _messageBuffer[nextSequence].Value;
                            messages[nextSequence - temp] = evt;
                            nextSequence++;
                        }
                        this._batchMessageHandler.Handle(messages);
                    }
                    _current.SetValue(availableSequence);
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }
    }
}
