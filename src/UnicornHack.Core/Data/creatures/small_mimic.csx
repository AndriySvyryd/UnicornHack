new Creature
{
    Name = "small mimic",
    Species = Species.Mimic,
    SpeciesClass = SpeciesClass.ShapeChanger,
    ArmorClass = 7,
    MovementDelay = 400,
    Weight = 300,
    Size = Size.Small,
    Nutrition = 200,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Touch, Timeout = 1, Effects = new HashSet<Effect> { new Stick { } } },
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new Polymorph { } } }
    }
,
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
    InitialLevel = 7,
    NextStageName = "large mimic",
    GenerationFrequency = Frequency.Occasionally
}
