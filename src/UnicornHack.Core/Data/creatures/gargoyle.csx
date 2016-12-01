new CreatureVariant
{
    Name = "gargoyle",
    Species = Species.Gargoyle,
    NextStageName = "winged gargoyle",
    InitialLevel = 6,
    ArmorClass = -4,
    MovementRate = 10,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 50,
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
,
    SimpleProperties = new HashSet<string> { "Humanoidness", "Breathlessness", "StoningResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    GenerationFrequency = Frequency.Commonly,
    Alignment = -9,
    Noise = ActorNoiseType.Grunt
}
