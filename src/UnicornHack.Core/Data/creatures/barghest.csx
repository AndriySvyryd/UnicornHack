new CreatureVariant
{
    InitialLevel = 9,
    ArmorClass = 2,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Mountable,
    Alignment = -6,
    Noise = ActorNoiseType.Bark,
    Name = "barghest",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine | SpeciesClass.ShapeChanger,
    MovementRate = 16,
    Size = Size.Medium,
    Weight = 1200,
    Nutrition = 500,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
    }
}
