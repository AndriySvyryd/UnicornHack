new Creature
{
    Name = "hill giant",
    Species = Species.Giant,
    ArmorClass = 6,
    MovementRate = 10,
    Weight = 2250,
    Size = Size.Huge,
    Nutrition = 750,
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
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    InitialLevel = 8,
    GenerationFlags = GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Alignment = -2,
    Noise = ActorNoiseType.Boast
}