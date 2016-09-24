new CreatureVariant
{
    InitialLevel = 4,
    ArmorClass = 8,
    MagicResistance = 30,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Hiss,
    NextStageName = "cockatrice",
    Name = "chickatrice",
    Species = Species.Cockatrice,
    SpeciesClass = SpeciesClass.MagicalBeast,
    MovementRate = 4,
    Size = Size.Tiny,
    Weight = 10,
    Nutrition = 10,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Omnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "StoningResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.Targetted, Action = AbilityAction.Touch, Timeout = 5, Effects = new AbilityEffect[] { new Stone() } },
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new Stone() } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Stone() } }
    }
}
