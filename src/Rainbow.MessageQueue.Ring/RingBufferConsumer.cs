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

        public RingBufferConsumer(
            IRingBuffer<TMessage> messageQueue,
            ISequenceBarrier sequenceBarrier,
            IMessageHandler<TMessage> batchMessageHandler)
        {
            this._messageBuffer = messageQueue;
            this._sequenceBarrier = sequenceBarrier;
            this._batchMessageHandler = batchMessageHandler;
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

            _sequenceBarrier.ClearAlert();
            var nextSequence = _current.Value + 1L;

            while (true)
            {
                try
                {
                    var availableSequence = _sequenceBarrier.WaitFor(nextSequence);
                    
                    while (nextSequence <= availableSequence)
                    {
                        this._batchMessageHandler.Handle(_messageBuffer[nextSequence].Value, nextSequence, nextSequence == availableSequence);
                        nextSequence++;
                    }

                    _current.SetValue(availableSequence);
                }
                catch (AlertException)
                {
                    if (_running == 0)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _current.SetValue(nextSequence);
                    nextSequence++;
                }
            }
        }
    }
}
