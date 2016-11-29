new CreatureVariant
{
    InitialLevel = 8,
    ArmorClass = 7,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Occasionally,
    PreviousStageName = "small mimic",
    NextStageName = "giant mimic",
    Name = "large mimic",
    Species = Species.Mimic,
    SpeciesClass = SpeciesClass.ShapeChanger,
    MovementRate = 3,
    Size = Size.Medium,
    Weight = 600,
    Nutrition = 400,
    SimpleProperties = new HashSet<string>
    {
        "Infravisibility",
        "Camouflage",
        "Eyelessness",
        "Headlessness",
        "Breathlessness",
        "Limblessness",
        "Clinginess",
        "Amorphism",
        "PolymorphControl",
        "Carnivorism"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "AcidResistance", 3 }, { "Stealthiness", 3 }, { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new Stick() } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Polymorph() } }
    }
}
