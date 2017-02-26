new Creature
{
    Name = "leprechaun",
    Species = Species.Leprechaun,
    SpeciesClass = SpeciesClass.Fey,
    ArmorClass = 8,
    MagicResistance = 20,
    MovementDelay = 80,
    Weight = 60,
    Size = Size.Tiny,
    Nutrition = 30,
    Abilities = new HashSet<Ability>
    {
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Claw, Timeout = 1, Effects = new HashSet<Effect> { new StealGold { } } }
    }
,
    SimpleProperties = new HashSet<string> { "Teleportation", "Infravisibility", "Omnivorism" },
    InitialLevel = 5,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.GoldCollector,
    Noise = ActorNoiseType.Laugh
}
