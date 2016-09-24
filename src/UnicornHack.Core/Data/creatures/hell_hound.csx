new CreatureVariant
{
    InitialLevel = 12,
    ArmorClass = 2,
    MagicResistance = 20,
    GenerationFlags = GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Usually,
    Alignment = -5,
    Noise = ActorNoiseType.Bark,
    PreviousStageName = "hell hound pup",
    Name = "hell hound",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    MovementRate = 14,
    Size = Size.Medium,
    Weight = 700,
    Nutrition = 300,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Breath,
            Timeout = 1,
            Effects = new AbilityEffect[] { new FireDamage { Damage = 10 } }
        }
    }
}
