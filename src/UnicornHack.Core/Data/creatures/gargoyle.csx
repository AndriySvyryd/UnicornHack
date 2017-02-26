new Creature
{
    Name = "gargoyle",
    Species = Species.Gargoyle,
    ArmorClass = -4,
    MovementDelay = 120,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 50,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Humanoidness", "Breathlessness", "StoningResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    InitialLevel = 6,
    NextStageName = "winged gargoyle",
    GenerationFrequency = Frequency.Commonly,
    Alignment = -9,
    Noise = ActorNoiseType.Grunt
}
