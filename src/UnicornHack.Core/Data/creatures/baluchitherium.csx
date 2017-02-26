new Creature
{
    Name = "baluchitherium",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    ArmorClass = 5,
    MovementDelay = 100,
    Weight = 3800,
    Size = Size.Large,
    Nutrition = 800,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 12 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 12 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    InitialLevel = 14,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Roar
}
