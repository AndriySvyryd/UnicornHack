namespace UnicornHack.Primitives;

[Flags]
public enum RoomReveal
{
    None = 0,
    Features = 1 << 0,
    CurseDetails = 1 << 1,
    CreatureCount = 1 << 2,
    CreatureDeityAlignment = 1 << 3,
    CreatureTypes = 1 << 4,
    Items = 1 << 5,
    Full = Features | CurseDetails | CreatureCount | CreatureDeityAlignment | CreatureTypes | Items
}
