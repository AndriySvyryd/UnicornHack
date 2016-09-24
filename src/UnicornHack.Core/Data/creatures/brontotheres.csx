new CreatureVariant
{
    InitialLevel = 12,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Roar,
    Name = "brontotheres",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    MovementRate = 12,
    Size = Size.Large,
    Weight = 2650,
    Nutrition = 650,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
        }
    }
}
