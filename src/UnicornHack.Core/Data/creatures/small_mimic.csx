new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = 7,
    GenerationFrequency = Frequency.Occasionally,
    NextStageName = "large mimic",
    Name = "small mimic",
    Species = Species.Mimic,
    SpeciesClass = SpeciesClass.ShapeChanger,
    MovementRate = 3,
    Size = Size.Small,
    Weight = 300,
    Nutrition = 200,
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new AbilityEffect[] { new Stick() } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Polymorph() } }
    }
}
