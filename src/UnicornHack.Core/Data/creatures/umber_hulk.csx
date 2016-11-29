new CreatureVariant
{
    InitialLevel = 9,
    ArmorClass = 2,
    MagicResistance = 25,
    GenerationFrequency = Frequency.Uncommonly,
    Name = "umber hulk",
    Species = Species.Hulk,
    SpeciesClass = SpeciesClass.MagicalBeast,
    MovementRate = 6,
    Size = Size.Large,
    Weight = 1300,
    Nutrition = 500,
    SimpleProperties = new HashSet<string> { "Tunneling", "AnimalBody", "Infravision", "Infravisibility", "Carnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
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
}
