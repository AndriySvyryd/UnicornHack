new Creature
{
    Name = "chickatrice",
    Species = Species.Cockatrice,
    SpeciesClass = SpeciesClass.MagicalBeast,
    ArmorClass = 8,
    MagicResistance = 30,
    MovementDelay = 300,
    Weight = 10,
    Size = Size.Tiny,
    Nutrition = 10,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 5, Effects = new HashSet<Effect> { new Stone { } } },
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> { new Stone { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Stone { } } }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Omnivorism", "SingularInventory", "StoningResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    InitialLevel = 4,
    NextStageName = "cockatrice",
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Hiss
}
