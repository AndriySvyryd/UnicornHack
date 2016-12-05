new Creature
{
    Name = "rothe",
    Species = Species.Quadruped,
    SpeciesClass = SpeciesClass.Quadrupedal,
    ArmorClass = 7,
    MovementRate = 9,
    Weight = 600,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 4 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Blindness", "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    InitialLevel = 2,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Sometimes,
    Noise = ActorNoiseType.Bleat
}
