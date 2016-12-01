new CreatureVariant
{
    Name = "leprechaun",
    Species = Species.Leprechaun,
    SpeciesClass = SpeciesClass.Fey,
    InitialLevel = 5,
    ArmorClass = 8,
    MagicResistance = 20,
    MovementRate = 15,
    Weight = 60,
    Size = Size.Tiny,
    Nutrition = 30,
    Abilities = new List<Ability>
    {
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Claw, Timeout = 1, Effects = new AbilityEffect[] { new StealGold() } }
    }
,
    SimpleProperties = new HashSet<string> { "Teleportation", "Infravisibility", "Omnivorism" },
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.GoldCollector,
    Noise = ActorNoiseType.Laugh
}
