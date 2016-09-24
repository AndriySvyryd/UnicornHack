new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 2,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking,
    Alignment = -7,
    Noise = ActorNoiseType.Cuss,
    CorpseVariantName = "",
    Name = "imp",
    Species = Species.Imp,
    SpeciesClass = SpeciesClass.Demon,
    MovementRate = 12,
    Size = Size.Tiny,
    Weight = 100,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravision", "Infravisibility" },
    ValuedProperties = new Dictionary<string, Object> { { "Regeneration", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } }
    }
}
