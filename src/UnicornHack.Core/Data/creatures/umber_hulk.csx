new CreatureVariant
{
    Name = "umber hulk",
    Species = Species.Hulk,
    SpeciesClass = SpeciesClass.MagicalBeast,
    InitialLevel = 9,
    ArmorClass = 2,
    MagicResistance = 25,
    MovementRate = 6,
    Weight = 1300,
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 6 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Gaze,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Confuse { Duration = 4 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Tunneling", "AnimalBody", "Infravision", "Infravisibility", "Carnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    GenerationFrequency = Frequency.Uncommonly
}
