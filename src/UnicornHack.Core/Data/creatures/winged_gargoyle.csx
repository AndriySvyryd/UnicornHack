new CreatureVariant
{
    InitialLevel = 9,
    ArmorClass = -4,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.MagicUser,
    Alignment = -12,
    Noise = ActorNoiseType.Grunt,
    PreviousStageName = "gargoyle",
    Name = "winged gargoyle",
    Species = Species.Gargoyle,
    MovementRate = 15,
    Size = Size.Medium,
    Weight = 1200,
    Nutrition = 50,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Humanoidness", "Breathlessness", "Oviparity" },
    ValuedProperties = new Dictionary<string, Object> { { "StoningResistance", 3 }, { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
}
