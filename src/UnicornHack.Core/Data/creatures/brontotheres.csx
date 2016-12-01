new CreatureVariant
{
    Name = "brontotheres",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    InitialLevel = 12,
    ArmorClass = 6,
    MovementRate = 12,
    Weight = 2650,
    Size = Size.Large,
    Nutrition = 650,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Roar
}
