new Creature
{
    Name = "brontotheres",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    ArmorClass = 6,
    MovementRate = 12,
    Weight = 2650,
    Size = Size.Large,
    Nutrition = 650,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 9 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    InitialLevel = 12,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Roar
}
