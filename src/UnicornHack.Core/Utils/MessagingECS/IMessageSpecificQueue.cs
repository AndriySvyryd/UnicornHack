namespace UnicornHack.Utils.MessagingECS;

public interface IMessageSpecificQueue
{
    void ProcessNext(IEntityManager manager);
}
