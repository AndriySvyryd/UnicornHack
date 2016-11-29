new CreatureVariant
{
    InitialLevel = 16,
    ArmorClass = -3,
    MagicResistance = 70,
    GenerationFlags = GenerationFlags.NonGenocidable,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser,
    Alignment = 9,
    Noise = ActorNoiseType.Boast,
    Name = "titan",
    Species = Species.Giant,
    MovementRate = 18,
    Size = Size.Gigantic,
    Weight = 3000,
    Nutrition = 900,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new MagicalDamage { Damage = 9 } }
        }
    }
}
