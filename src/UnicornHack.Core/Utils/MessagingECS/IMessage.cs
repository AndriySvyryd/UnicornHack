namespace UnicornHack.Utils.MessagingECS;

public interface IMessage
{
    string MessageName
    {
        get;
        set;
    }

    void Clean();
}
