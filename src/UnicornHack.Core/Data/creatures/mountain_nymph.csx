new CreatureVariant
{
    Name = "mountain nymph",
    Species = Species.Nymph,
    SpeciesClass = SpeciesClass.Fey,
    InitialLevel = 3,
    ArmorClass = 9,
    MagicResistance = 20,
    MovementRate = 12,
    Weight = 600,
    Size = Size.Medium,
    Nutrition = 300,
    Abilities = new List<Ability>
    {
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new Seduce() } },
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new StealItem() } }
    }
,
    SimpleProperties = new HashSet<string> { "Teleportation", "Humanoidness", "Infravisibility", "Femaleness" },
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Seduction
}
