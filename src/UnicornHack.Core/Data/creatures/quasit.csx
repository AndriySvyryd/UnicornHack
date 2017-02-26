new Creature
{
    Name = "quasit",
    Species = Species.Imp,
    SpeciesClass = SpeciesClass.Demon,
    ArmorClass = 2,
    MagicResistance = 20,
    MovementDelay = 80,
    Weight = 200,
    Size = Size.Small,
    Nutrition = 100,
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
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new DrainDexterity { Amount = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 3 } } }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 } },
    InitialLevel = 3,
    CorpseName = "",
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking,
    Alignment = -7,
    Noise = ActorNoiseType.Cuss
}
