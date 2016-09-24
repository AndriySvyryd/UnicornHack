new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 7,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Stalking,
    Alignment = -7,
    Noise = ActorNoiseType.Hiss,
    CorpseVariantName = "",
    Name = "lemure",
    Species = Species.Homunculus,
    SpeciesClass = SpeciesClass.Demon,
    MovementRate = 3,
    Size = Size.Medium,
    Weight = 500,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Infravisibility" },
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 2 } } }
    }
}
