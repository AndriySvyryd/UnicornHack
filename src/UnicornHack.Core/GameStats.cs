using UnicornHack.Utils;

namespace UnicornHack;

public class GameStats : NotificationEntity
{
    public int DeathCount
    {
        get;
        set => SetWithNotify(value, ref field);
    }
}