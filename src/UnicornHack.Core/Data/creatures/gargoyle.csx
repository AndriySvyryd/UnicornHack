new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = -4,
    GenerationFrequency = Frequency.Commonly,
    Alignment = -9,
    Noise = ActorNoiseType.Grunt,
    NextStageName = "winged gargoyle",
    Name = "gargoyle",
    Species = Species.Gargoyle,
    MovementRate = 10,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "Humanoidness", "Breathlessness" },
    ValuedProperties = new Dictionary<string, Object> { { "StoningResistance", 3 }, { "ThickHide", 3 } },
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
    }
}
