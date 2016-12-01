new CreatureVariant
{
    Name = "disenchanter",
    Species = Species.Disenchanter,
    InitialLevel = 12,
    ArmorClass = -10,
    MagicResistance = 30,
    MovementRate = 12,
    Weight = 750,
    Size = Size.Medium,
    Nutrition = 200,
    Abilities = new List<Ability>
    {
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new Disenchant() } },
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new Disenchant() } }
    }
,
    SimpleProperties = new HashSet<string> { "Infravisibility", "AnimalBody", "Handlessness", "Metallivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Occasionally,
    Noise = ActorNoiseType.Growl
}
