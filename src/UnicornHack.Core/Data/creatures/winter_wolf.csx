new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = 4,
    MagicResistance = 20,
    GenerationFlags = GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Commonly,
    Alignment = -5,
    Noise = ActorNoiseType.Bark,
    PreviousStageName = "winter wolf cub",
    Name = "winter wolf",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 700,
    Nutrition = 300,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Breath,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ColdDamage { Damage = 7 } }
        }
    }
}
