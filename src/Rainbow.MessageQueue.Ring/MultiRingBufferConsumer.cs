using System;
using System.Threading;

namespace Rainbow.MessageQueue.Ring
{
    public class MultiRingBufferConsumer<TMessage> : IRingBufferConsumer
    {

        private volatile int _running;
        private readonly IRingBuffer<TMessage>[] _messageBuffers;
        private readonly ISequenceBarrier[] _sequenceBarriers;
        private readonly IMessageHandler<TMessage> _batchMessageHandler;
        private readonly Sequence[] _sequences;
        private readonly int _maxHandleSize;
        private long _count;

        private const int _defaultMaxHandleSize = 100;

        public MultiRingBufferConsumer(
            IRingBuffer<TMessage>[] messageBuffers,
            ISequenceBarrier[] sequenceBarriers,
            IMessageHandler<TMessage> batchMessageHandler,
            int maxHandleSize = _defaultMaxHandleSize)
        {
            if (maxHandleSize < 0) throw new ArgumentException($"{nameof(maxHandleSize)} must greater than 0", nameof(maxHandleSize));

            this._messageBuffers = messageBuffers;
            this._sequenceBarriers = sequenceBarriers;
            this._batchMessageHandler = batchMessageHandler;

            _sequences = new Sequence[messageBuffers.Length];
            for (var i = 0; i < _sequences.Length; i++)
            {
                _sequences[i] = new Sequence();
            }
        }


        public Sequence Sequence => throw new NotSupportedException();

        public Sequence[] GetSequences()
        {
            return _sequences;
        }

        public bool IsRunning => this._running == 1;

        public void Halt()
        {
            _running = 0;
            _sequenceBarriers[0].Alert();
        }

        public void Run()
        {
            if (Interlocked.Exchange(ref _running, 1) != 0)
                throw new InvalidOperationException("Thread is already running");


            var barrierLength = _sequenceBarriers.Length;

            while (true)
            {
                try
                {
                    for (var i = 0; i < barrierLength; i++)
                    {
                        var availableSequence = _sequenceBarriers[i].WaitFor(-1);
                        var sequence = _sequences[i];

                        var nextSequence = sequence.Value + 1;

                        availableSequence = availableSequence > nextSequence + _maxHandleSize ? nextSequence + _maxHandleSize : availableSequence;


                        if (nextSequence <= availableSequence)
                        {
                            TMessage[] messages = new TMessage[availableSequence - nextSequence];

                            for (var l = nextSequence; l <= availableSequence; l++)
                            {
                                var evt = _messageBuffers[i][l].Value;
                                messages[l - nextSequence] = evt;
                            }
                            this._batchMessageHandler.Handle(messages);
                        }

                        sequence.SetValue(availableSequence);

                        _count += availableSequence - nextSequence + 1;
                    }
                }
                catch (TimeoutException e)
                {
                }
                catch (Exception e)
                {
                    break;
                }
            }
        }
    }
}