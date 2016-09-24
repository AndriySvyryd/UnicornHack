new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 4,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Imitate,
    Name = "leocrotta",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    MovementRate = 18,
    Size = Size.Large,
    Weight = 1200,
    Nutrition = 500,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Omnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
}
