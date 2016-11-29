new CreatureVariant
{
    InitialLevel = 8,
    ArmorClass = 2,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Usually,
    Noise = ActorNoiseType.Burble,
    Name = "wumpus",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Aberration,
    MovementRate = 3,
    Size = Size.Large,
    Weight = 2500,
    Nutrition = 500,
    SimpleProperties = new HashSet<string> { "Clinginess", "AnimalBody", "Infravisibility", "Handlessness", "Omnivorism", "SingularInventory" },
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
}
