new CreatureVariant
{
    Name = "hell hound pup",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    NextStageName = "hell hound",
    InitialLevel = 7,
    ArmorClass = 4,
    MagicResistance = 20,
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Breath,
            Timeout = 1,
            Effects = new AbilityEffect[] { new FireDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 } },
    GenerationFlags = GenerationFlags.SmallGroup | GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -5,
    Noise = ActorNoiseType.Bark
}
