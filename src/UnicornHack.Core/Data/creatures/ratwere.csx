new Creature
{
    Name = "ratwere",
    Species = Species.Rat,
    SpeciesClass = SpeciesClass.Rodent | SpeciesClass.ShapeChanger,
    ArmorClass = 6,
    MagicResistance = 10,
    MovementDelay = 100,
    Weight = 150,
    Size = Size.Small,
    Nutrition = 50,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 5,
            Effects = new HashSet<Effect> { new ConferLycanthropy { VariantName = "ratwere" } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new ConferLycanthropy { VariantName = "ratwere" } } }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 } },
    InitialLevel = 3,
    CorpseName = "",
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Alignment = -7,
    Noise = ActorNoiseType.Sqeek
}
