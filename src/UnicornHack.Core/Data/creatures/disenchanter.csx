new Creature
{
    Name = "disenchanter",
    Species = Species.Disenchanter,
    ArmorClass = -10,
    MagicResistance = 30,
    MovementRate = 12,
    Weight = 750,
    Size = Size.Medium,
    Nutrition = 200,
    Abilities = new HashSet<Ability>
    {
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new HashSet<Effect> { new Disenchant { } } },
        new Ability { Activation = AbilityActivation.OnMeleeHit, Effects = new HashSet<Effect> { new Disenchant { } } }
    }
,
    SimpleProperties = new HashSet<string> { "Infravisibility", "AnimalBody", "Handlessness", "Metallivorism", "SingularInventory" },
    InitialLevel = 12,
    GenerationFrequency = Frequency.Occasionally,
    Noise = ActorNoiseType.Growl
}
