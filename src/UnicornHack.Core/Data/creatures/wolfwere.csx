new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 4,
    MagicResistance = 20,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Alignment = -7,
    Noise = ActorNoiseType.Bark,
    CorpseVariantName = "",
    Name = "wolfwere",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine | SpeciesClass.ShapeChanger,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 500,
    Nutrition = 250,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 } },
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
            Action = AbilityAction.Bite,
            Timeout = 5,
            Effects = new AbilityEffect[] { new ConferLycanthropy { VariantName = "wolfwere" } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new ConferLycanthropy { VariantName = "wolfwere" } } }
    }
}
