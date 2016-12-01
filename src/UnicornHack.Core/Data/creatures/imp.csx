new CreatureVariant
{
    Name = "imp",
    Species = Species.Imp,
    SpeciesClass = SpeciesClass.Demon,
    CorpseVariantName = "",
    InitialLevel = 3,
    ArmorClass = 2,
    MagicResistance = 20,
    MovementRate = 12,
    Weight = 100,
    Size = Size.Tiny,
    Nutrition = 50,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravision", "Infravisibility" },
    ValuedProperties = new Dictionary<string, Object> { { "Regeneration", 3 } },
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking,
    Alignment = -7,
    Noise = ActorNoiseType.Cuss
}
