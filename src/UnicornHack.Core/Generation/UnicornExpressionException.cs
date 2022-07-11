namespace UnicornHack.Generation;

public class UnicornExpressionException : Exception
{
    public UnicornExpressionException()
    {
    }

    public UnicornExpressionException(Exception innerException)
        : base("", innerException)
    {
    }
}
