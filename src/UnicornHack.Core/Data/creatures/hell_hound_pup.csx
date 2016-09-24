new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = 4,
    MagicResistance = 20,
    GenerationFlags = GenerationFlags.SmallGroup | GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -5,
    Noise = ActorNoiseType.Bark,
    NextStageName = "hell hound",
    Name = "hell hound pup",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    MovementRate = 12,
    Size = Size.Small,
    Weight = 250,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Breath,
            Timeout = 1,
            Effects = new AbilityEffect[] { new FireDamage { Damage = 7 } }
        }
    }
}
