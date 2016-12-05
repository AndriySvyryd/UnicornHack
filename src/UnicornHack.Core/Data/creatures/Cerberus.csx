new Creature
{
    Name = "Cerberus",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    ArmorClass = 2,
    MagicResistance = 20,
    MovementRate = 10,
    Weight = 1000,
    Size = Size.Large,
    Nutrition = 350,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "Maleness", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 } },
    InitialLevel = 13,
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.HellOnly,
    GenerationFrequency = Frequency.Once,
    Alignment = -7,
    Noise = ActorNoiseType.Bark
}
