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
        private long _count;


        public MultiRingBufferConsumer(
            IRingBuffer<TMessage>[] messageBuffers,
            ISequenceBarrier[] sequenceBarriers,
            IMessageHandler<TMessage> batchMessageHandler)
        {
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
            var currentBarrierIndex = 0;
            long nextSequence = -1;
            while (true)
            {
                try
                {
                    for (var i = 0; i < barrierLength; i++)
                    {
                        currentBarrierIndex = i;
                        var availableSequence = _sequenceBarriers[i].WaitFor(-1);
                        var sequence = _sequences[i];

                        while (availableSequence > (nextSequence = sequence.Value + 1))
                        {
                            // var maxSequence = availableSequence > nextSequence + _maxHandleSize ? nextSequence + _maxHandleSize : availableSequence;

                            while (nextSequence <= availableSequence)
                            {
                                this._batchMessageHandler.Handle(_messageBuffers[i][nextSequence].Value, nextSequence, nextSequence == availableSequence);
                                nextSequence++;
                            }

                            sequence.SetValue(nextSequence);
                        }

                        _count += availableSequence - nextSequence + 1;
                    }
                }
                catch (AlertException)
                {
                    if (_running == 0)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    _sequences[currentBarrierIndex].SetValue(nextSequence);
                }
            }
        }
    }
}