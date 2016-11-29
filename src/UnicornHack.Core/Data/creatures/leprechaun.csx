new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 8,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.GoldCollector,
    Noise = ActorNoiseType.Laugh,
    Name = "leprechaun",
    Species = Species.Leprechaun,
    SpeciesClass = SpeciesClass.Fey,
    MovementRate = 15,
    Size = Size.Tiny,
    Weight = 60,
    Nutrition = 30,
    SimpleProperties = new HashSet<string> { "Teleportation", "Infravisibility", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Claw, Timeout = 1, Effects = new AbilityEffect[] { new StealGold() } }
    }
}
