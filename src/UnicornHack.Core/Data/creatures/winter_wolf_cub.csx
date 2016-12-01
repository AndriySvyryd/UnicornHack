new CreatureVariant
{
    Name = "winter wolf cub",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine,
    NextStageName = "winter wolf",
    InitialLevel = 5,
    ArmorClass = 4,
    MovementRate = 12,
    Weight = 250,
    Size = Size.Small,
    Nutrition = 200,
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
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 } },
    GenerationFlags = GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Commonly,
    Alignment = -5,
    Noise = ActorNoiseType.Bark
}
