new CreatureVariant
{
    Name = "leocrotta",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    InitialLevel = 6,
    ArmorClass = 4,
    MagicResistance = 10,
    MovementRate = 18,
    Weight = 1200,
    Size = Size.Large,
    Nutrition = 500,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Omnivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Imitate
}
