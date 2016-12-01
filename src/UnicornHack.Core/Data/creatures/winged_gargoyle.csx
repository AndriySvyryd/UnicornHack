new CreatureVariant
{
    Name = "winged gargoyle",
    Species = Species.Gargoyle,
    PreviousStageName = "gargoyle",
    InitialLevel = 9,
    ArmorClass = -4,
    MovementRate = 15,
    Weight = 1200,
    Size = Size.Medium,
    Nutrition = 50,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Humanoidness", "Breathlessness", "Oviparity", "StoningResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "ThickHide", 3 } },
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.MagicUser,
    Alignment = -12,
    Noise = ActorNoiseType.Grunt
}
