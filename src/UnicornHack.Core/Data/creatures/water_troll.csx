new Creature
{
    Name = "water troll",
    Species = Species.Troll,
    ArmorClass = 4,
    MagicResistance = 40,
    MovementDelay = 85,
    Weight = 1200,
    Size = Size.Large,
    Nutrition = 350,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 9 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 9 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Swimming", "Infravision", "Infravisibility", "Humanoidness", "Carnivorism", "Reanimation" },
    ValuedProperties = new Dictionary<string, Object> { { "Regeneration", 3 } },
    InitialLevel = 11,
    GenerationFlags = GenerationFlags.NoHell,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector,
    Alignment = -3,
    Noise = ActorNoiseType.Grunt
}
