new Creature
{
    Name = "leocrotta",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    ArmorClass = 4,
    MagicResistance = 10,
    MovementDelay = 66,
    Weight = 1200,
    Size = Size.Large,
    Nutrition = 500,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
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
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Omnivorism", "SingularInventory" },
    InitialLevel = 6,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Imitate
}
