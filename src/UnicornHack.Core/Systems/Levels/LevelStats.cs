using UnicornHack.Utils;

namespace UnicornHack.Systems.Levels;

public class LevelStats : NotificationEntity
{
    public int DeathCount
    {
        get;
        set => SetWithNotify(value, ref field);
    }
}