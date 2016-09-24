new CreatureVariant
{
    InitialLevel = 10,
    ArmorClass = 3,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Boast,
    Name = "ettin",
    Species = Species.Giant,
    MovementRate = 12,
    Size = Size.Huge,
    Weight = 2250,
    Nutrition = 750,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
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
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}
