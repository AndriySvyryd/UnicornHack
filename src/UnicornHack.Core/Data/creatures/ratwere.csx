new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 6,
    MagicResistance = 10,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Alignment = -7,
    Noise = ActorNoiseType.Sqeek,
    CorpseVariantName = "",
    Name = "ratwere",
    Species = Species.Rat,
    SpeciesClass = SpeciesClass.Rodent | SpeciesClass.ShapeChanger,
    MovementRate = 12,
    Size = Size.Small,
    Weight = 150,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 5,
            Effects = new AbilityEffect[] { new ConferLycanthropy { VariantName = "ratwere" } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new ConferLycanthropy { VariantName = "ratwere" } } }
    }
}
