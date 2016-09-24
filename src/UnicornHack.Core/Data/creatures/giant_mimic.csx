new CreatureVariant
{
    InitialLevel = 9,
    ArmorClass = 7,
    MagicResistance = 20,
    GenerationFrequency = Frequency.Occasionally,
    PreviousStageName = "large mimic",
    Name = "giant mimic",
    Species = Species.Mimic,
    SpeciesClass = SpeciesClass.ShapeChanger,
    MovementRate = 3,
    Size = Size.Large,
    Weight = 800,
    Nutrition = 500,
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
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability { Activation = AbilityActivation.Targetted, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new Stick() } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Polymorph() } }
    }
}
