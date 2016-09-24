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
    Name = "quasit",
    Species = Species.Imp,
    SpeciesClass = SpeciesClass.Demon,
    MovementRate = 15,
    Size = Size.Small,
    Weight = 200,
    Nutrition = 100,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 } },
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
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new DrainDexterity { Amount = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 3 } } }
    }
}
