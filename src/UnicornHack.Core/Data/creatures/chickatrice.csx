new CreatureVariant
{
    Name = "chickatrice",
    Species = Species.Cockatrice,
    SpeciesClass = SpeciesClass.MagicalBeast,
    NextStageName = "cockatrice",
    InitialLevel = 4,
    ArmorClass = 8,
    MagicResistance = 30,
    MovementRate = 4,
    Weight = 10,
    Size = Size.Tiny,
    Nutrition = 10,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 5, Effects = new AbilityEffect[] { new Stone() } },
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new Stone() } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Stone() } }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Omnivorism", "SingularInventory", "StoningResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Hiss
}
