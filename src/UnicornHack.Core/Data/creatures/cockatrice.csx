new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 6,
    MagicResistance = 30,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Occasionally,
    Noise = ActorNoiseType.Hiss,
    PreviousStageName = "chickatrice",
    Name = "cockatrice",
    Species = Species.Cockatrice,
    SpeciesClass = SpeciesClass.MagicalBeast,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 30,
    Nutrition = 30,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Oviparity", "Omnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "StoningResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new Stone() } },
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new Stone() } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Stone() } }
    }
}
