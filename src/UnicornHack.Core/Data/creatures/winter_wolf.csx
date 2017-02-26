new Creature
{
    Name = "winter wolf",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine,
    ArmorClass = 4,
    MagicResistance = 20,
    MovementDelay = 100,
    Weight = 700,
    Size = Size.Medium,
    Nutrition = 300,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Breath,
            Timeout = 1,
            Effects = new HashSet<Effect> { new ColdDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 } },
    InitialLevel = 7,
    PreviousStageName = "winter wolf cub",
    GenerationFlags = GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Commonly,
    Alignment = -5,
    Noise = ActorNoiseType.Bark
}
