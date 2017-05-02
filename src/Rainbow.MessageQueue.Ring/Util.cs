using System;
using System.Collections.Generic;

namespace Rainbow.MessageQueue.Ring
{
    public class Util
    {
        public static int Log2(int i)
        {
            var r = 0;
            while ((i >>= 1) != 0)
            {
                ++r;
            }
            return r;
        }

        public static int CeilingNextPowerOfTwo(int x)
        {
            var result = 2;

            while (result < x)
            {
                result <<= 1;
            }

            return result;
        }


        public static long GetMinimum(IEnumerable<ISequence> gatingSequences, long minimum = long.MaxValue)
        {
            foreach (var item in gatingSequences)
            {
                minimum = Math.Min(minimum, item.Value);
            }
            return minimum;
        }
    }
}