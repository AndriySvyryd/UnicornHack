new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 7,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Bleat,
    Name = "rothe",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    MovementRate = 9,
    Size = Size.Medium,
    Weight = 600,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Blindness", "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
    }
}
