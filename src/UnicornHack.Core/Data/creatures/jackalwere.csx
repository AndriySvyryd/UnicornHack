new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 7,
    MagicResistance = 10,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Alignment = -7,
    Noise = ActorNoiseType.Bark,
    CorpseVariantName = "",
    Name = "jackalwere",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine | SpeciesClass.ShapeChanger,
    MovementRate = 12,
    Size = Size.Small,
    Weight = 300,
    Nutrition = 250,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 5,
            Effects = new AbilityEffect[] { new ConferLycanthropy { VariantName = "jackalwere" } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new ConferLycanthropy { VariantName = "jackalwere" } } }
    }
}
