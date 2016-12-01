new CreatureVariant
{
    Name = "doppelganger",
    Species = Species.Doppelganger,
    SpeciesClass = SpeciesClass.ShapeChanger,
    InitialLevel = 9,
    ArmorClass = 5,
    MagicResistance = 20,
    MovementRate = 12,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
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
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "PolymorphControl", "Infravisibility", "Humanoidness", "Omnivorism" },
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Imitate
}
