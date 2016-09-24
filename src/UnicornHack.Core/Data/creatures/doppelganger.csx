new CreatureVariant
{
    InitialLevel = 9,
    ArmorClass = 5,
    MagicResistance = 20,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Imitate,
    Name = "doppelganger",
    Species = Species.Doppelganger,
    SpeciesClass = SpeciesClass.ShapeChanger,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "SleepResistance", "PolymorphControl", "Infravisibility", "Humanoidness", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 6 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}
