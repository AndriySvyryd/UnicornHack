new CreatureVariant
{
    InitialLevel = 13,
    ArmorClass = 2,
    MagicResistance = 20,
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Once,
    Alignment = -7,
    Noise = ActorNoiseType.Bark,
    Name = "Cerberus",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    MovementRate = 10,
    Size = Size.Large,
    Weight = 1000,
    Nutrition = 350,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "Maleness", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
    }
}
