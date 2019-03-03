using System.Collections.Generic;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class TestRandom : SimpleRandom
    {
        private Queue<bool> _boolsToReturn;

        public void EnqueueNextBool(bool value)
        {
            if (_boolsToReturn == null)
            {
                _boolsToReturn = new Queue<bool>();
            }

            _boolsToReturn.Enqueue(value);
        }

        public override bool NextBool(int successProbability, ref int entropyState)
        {
            if (_boolsToReturn != null
                && _boolsToReturn.Count > 0)
            {
                return _boolsToReturn.Dequeue();
            }

            return base.NextBool(successProbability, ref entropyState);
        }
    }
}
