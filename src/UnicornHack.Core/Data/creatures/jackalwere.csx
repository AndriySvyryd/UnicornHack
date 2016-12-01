new CreatureVariant
{
    Name = "jackalwere",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine | SpeciesClass.ShapeChanger,
    CorpseVariantName = "",
    InitialLevel = 2,
    ArmorClass = 7,
    MagicResistance = 10,
    MovementRate = 12,
    Weight = 300,
    Size = Size.Small,
    Nutrition = 250,
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
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 } },
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Alignment = -7,
    Noise = ActorNoiseType.Bark
}
