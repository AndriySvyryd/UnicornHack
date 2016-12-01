new CreatureVariant
{
    Name = "trapper",
    Species = Species.Trapper,
    InitialLevel = 12,
    ArmorClass = 3,
    MovementRate = 3,
    Weight = 800,
    Size = Size.Large,
    Nutrition = 350,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Engulf { Duration = 5 } }
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
,
    SimpleProperties = new HashSet<string> { "Camouflage", "AnimalBody", "InvisibilityDetection", "Eyelessness", "Headlessness", "Limblessness", "Carnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Stalking
}
