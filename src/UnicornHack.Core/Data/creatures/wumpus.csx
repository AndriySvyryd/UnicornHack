new CreatureVariant
{
    Name = "wumpus",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Aberration,
    InitialLevel = 8,
    ArmorClass = 2,
    MagicResistance = 10,
    MovementRate = 3,
    Weight = 2500,
    Size = Size.Large,
    Nutrition = 500,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Clinginess", "AnimalBody", "Infravisibility", "Handlessness", "Omnivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Burble
}
