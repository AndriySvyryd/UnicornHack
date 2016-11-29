new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 4,
    GenerationFlags = GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Commonly,
    Alignment = -5,
    Noise = ActorNoiseType.Bark,
    NextStageName = "winter wolf",
    Name = "winter wolf cub",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine,
    MovementRate = 12,
    Size = Size.Small,
    Weight = 250,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Breath,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ColdDamage { Damage = 4 } }
        }
    }
}
