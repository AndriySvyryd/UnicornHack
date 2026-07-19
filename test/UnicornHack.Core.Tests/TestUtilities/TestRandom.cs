using UnicornHack.Utils;

namespace UnicornHack;

public class TestRandom : SimpleRandom
{
    private Queue<bool>? _boolsToReturn;
    private Queue<int>? _intsToReturn;

    public void EnqueueNextBool(bool value)
    {
        if (_boolsToReturn == null)
        {
            _boolsToReturn = new Queue<bool>();
        }

        _boolsToReturn.Enqueue(value);
    }

    public void EnqueueNext(int value)
    {
        if (_intsToReturn == null)
        {
            _intsToReturn = new Queue<int>();
        }

        _intsToReturn.Enqueue(value);
    }

    public override int Next(int minValue, int maxValue)
    {
        if (_intsToReturn != null
            && _intsToReturn.Count > 0)
        {
            return _intsToReturn.Dequeue();
        }

        return base.Next(minValue, maxValue);
    }

    public override bool NextStreaklessBool(int successProbability, ref int karma)
    {
        switch (successProbability)
        {
            case 0:
                return false;
            case 100:
                return true;
        }

        if (_boolsToReturn != null
            && _boolsToReturn.Count > 0)
        {
            return _boolsToReturn.Dequeue();
        }

        return base.NextStreaklessBool(successProbability, ref karma);
    }

    public override bool NextStreaklessPatternlessBool(int successProbability, ref int entropyState)
    {
        switch (successProbability)
        {
            case 0:
                return false;
            case 100:
                return true;
        }

        if (_boolsToReturn != null
            && _boolsToReturn.Count > 0)
        {
            return _boolsToReturn.Dequeue();
        }

        return base.NextStreaklessPatternlessBool(successProbability, ref entropyState);
    }
}
