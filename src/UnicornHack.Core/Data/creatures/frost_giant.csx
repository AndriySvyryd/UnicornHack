new Creature
{
    Name = "frost giant",
    Species = Species.Giant,
    ArmorClass = 3,
    MagicResistance = 10,
    MovementDelay = 100,
    Weight = 2250,
    Size = Size.Huge,
    Nutrition = 750,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 11 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 } },
    InitialLevel = 10,
    GenerationFlags = GenerationFlags.SmallGroup | GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -3,
    Noise = ActorNoiseType.Boast
}
