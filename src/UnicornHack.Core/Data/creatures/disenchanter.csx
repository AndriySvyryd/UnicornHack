new CreatureVariant
{
    InitialLevel = 12,
    ArmorClass = -10,
    MagicResistance = 30,
    GenerationFrequency = Frequency.Occasionally,
    Noise = ActorNoiseType.Growl,
    Name = "disenchanter",
    Species = Species.Disenchanter,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 750,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "Infravisibility", "AnimalBody", "Handlessness", "Metallivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new Disenchant() } },
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new AbilityEffect[] { new Disenchant() } }
    }
}
