using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System;

namespace Rainbow.MessageQueue.Ring
{
    public sealed class SequenceBarrier : ISequenceBarrier
    {
        private readonly ISequencer _sequencer;
        private readonly ISequence _cursor;
        private readonly ISequence _dependent;
        private readonly IWaitStrategy _waitStrategy;
        private volatile bool _alerted;


        public SequenceBarrier(ISequencer sequencer, IWaitStrategy waitStrategy, ISequence cursor, IEnumerable<ISequence> dependents)
        {
            this._sequencer = sequencer;
            this._waitStrategy = waitStrategy;
            this._cursor = cursor;
            this._dependent = !dependents.Any() ? cursor : new SequenceGroup(dependents);
        }

        public long Cursor => _dependent.Value;

        public bool IsAlerted => _alerted;

        public void Alert()
        {
            _alerted = true;
            //_waitStrategy.SignalAllWhenBlocking();
        }

        public void CheckAlert()
        {
            if (_alerted)
            {
                throw new AlertException();
            }
        }

        public void ClearAlert()
        {
            _alerted = false;
        }

        public long WaitFor(long sequence)
        {
            while (_dependent.Value < sequence)
            {
                _waitStrategy.WaitFor(sequence, _cursor, _dependent, this);
            }
            return _sequencer.GetAvailableSequence(sequence, _dependent.Value);
        }
    }
}