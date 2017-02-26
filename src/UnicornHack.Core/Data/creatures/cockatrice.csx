new Creature
{
    Name = "cockatrice",
    Species = Species.Cockatrice,
    SpeciesClass = SpeciesClass.MagicalBeast,
    ArmorClass = 6,
    MagicResistance = 30,
    MovementDelay = 200,
    Weight = 30,
    Size = Size.Small,
    Nutrition = 30,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new HashSet<Effect> { new Stone { } } },
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> { new Stone { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Stone { } } }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Oviparity", "Omnivorism", "SingularInventory", "StoningResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 5,
    PreviousStageName = "chickatrice",
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Occasionally,
    Noise = ActorNoiseType.Hiss
}
