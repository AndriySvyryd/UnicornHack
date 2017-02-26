new Creature
{
    Name = "mumak",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    MovementDelay = 133,
    Weight = 2500,
    Size = Size.Large,
    Nutrition = 1000,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 13 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    InitialLevel = 5,
    GenerationFrequency = Frequency.Sometimes,
    Alignment = -2,
    Noise = ActorNoiseType.Roar
}
