new CreatureVariant
{
    Name = "barghest",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine | SpeciesClass.ShapeChanger,
    InitialLevel = 9,
    ArmorClass = 2,
    MagicResistance = 20,
    MovementRate = 16,
    Weight = 1200,
    Size = Size.Medium,
    Nutrition = 500,
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
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.AlignmentAware | MonsterBehavior.Mountable,
    Alignment = -6,
    Noise = ActorNoiseType.Bark
}
