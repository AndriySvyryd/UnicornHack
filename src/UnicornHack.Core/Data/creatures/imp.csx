new Creature
{
    Name = "imp",
    Species = Species.Imp,
    SpeciesClass = SpeciesClass.Demon,
    ArmorClass = 2,
    MagicResistance = 20,
    MovementRate = 12,
    Weight = 100,
    Size = Size.Tiny,
    Nutrition = 50,
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
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravision", "Infravisibility" },
    ValuedProperties = new Dictionary<string, Object> { { "Regeneration", 3 } },
    InitialLevel = 3,
    CorpseName = "",
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking,
    Alignment = -7,
    Noise = ActorNoiseType.Cuss
}
