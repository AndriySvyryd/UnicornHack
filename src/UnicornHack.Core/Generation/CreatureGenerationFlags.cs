namespace UnicornHack.Generation;

[Flags]
public enum CreatureGenerationFlags
{
    None,
    NonGenocidable = 1 << 1,
    NonPolymorphable = 1 << 2,
    SmallGroup = 1 << 3,
    LargeGroup = 1 << 4,
    Entourage = 1 << 5,
    /// <summary>A game-unique boss at the end of a level.</summary>
    LevelBoss = 1 << 6,
    /// <summary>A game-unique boss at the end of a branch.</summary>
    BranchBoss = 1 << 7
}
