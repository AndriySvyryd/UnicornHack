new CreatureVariant
{
    InitialLevel = 10,
    ArmorClass = 3,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Stalking,
    Name = "lurker above",
    Species = Species.Trapper,
    MovementRate = 3,
    Size = Size.Large,
    Weight = 800,
    Nutrition = 350,
    SimpleProperties = new HashSet<string>
    {
        "Flight",
        "FlightControl",
        "Camouflage",
        "AnimalBody",
        "Eyelessness",
        "Headlessness",
        "Limblessness",
        "Clinginess",
        "Carnivorism"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Engulf { Duration = 4 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnDigestion,
            Action = AbilityAction.Digestion,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Suffocate() }
        }
    }
}
