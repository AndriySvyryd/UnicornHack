namespace UnicornHack.Data.Creatures;

public static partial class CreatureData
{
    public static readonly Creature GnomeWizard = new Creature
    {
        Name = "gnome wizard",
        SpeciesClass = SpeciesClass.Humanoid,
        InitialLevel = 3,
        GenerationFlags = CreatureGenerationFlags.NonPolymorphable,
        Behavior = AIBehavior.GoldCollector | AIBehavior.WeaponCollector | AIBehavior.MagicUser,
        Noise = ActorNoiseType.Speach,
        Sex = Sex.Male,
        Size = 2,
        Weight = 700,
        MovementDelay = 50,
        TurningDelay = 50,
        PerceptionBonus = -8,
        MightBonus = -8,
        SpeedBonus = -8,
        FocusBonus = -8,
        MagicResistance = 10,
        TorsoType = TorsoType.Humanoid,
        UpperExtremities = ExtremityType.GraspingFingers,
        LowerExtremities = ExtremityType.Fingers,
        Infravisible = true,
        Infravision = true
    };
}
