new Creature
{
    Name = "mane",
    Species = Species.Homunculus,
    SpeciesClass = SpeciesClass.Demon,
    ArmorClass = 7,
    MovementDelay = 400,
    Weight = 500,
    Size = Size.Medium,
    Nutrition = 200,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 2 } } }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Infravisibility" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 3,
    CorpseName = "",
    GenerationFlags = GenerationFlags.LargeGroup,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Stalking,
    Alignment = -7,
    Noise = ActorNoiseType.Hiss
}
