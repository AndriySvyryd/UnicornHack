new CreatureVariant
{
    InitialLevel = 11,
    ArmorClass = 4,
    MagicResistance = 40,
    GenerationFlags = GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
    Alignment = -3,
    Noise = ActorNoiseType.Grunt,
    Name = "water troll",
    Species = Species.Troll,
    MovementRate = 14,
    Size = Size.Large,
    Weight = 1200,
    Nutrition = 350,
    SimpleProperties = new HashSet<string> { "Swimming", "Infravision", "Infravisibility", "Humanoidness", "Carnivorism", "Reanimation" },
    ValuedProperties = new Dictionary<string, Object> { { "Regeneration", 3 } },
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
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
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
