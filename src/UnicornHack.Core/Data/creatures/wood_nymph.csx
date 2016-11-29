new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 9,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Seduction,
    Name = "wood nymph",
    Species = Species.Nymph,
    SpeciesClass = SpeciesClass.Fey,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 600,
    Nutrition = 300,
    SimpleProperties = new HashSet<string> { "Teleportation", "Humanoidness", "Infravisibility", "Femaleness" },
    Abilities = new List<Ability>
    {
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new Seduce() } },
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new StealItem() } }
    }
}
