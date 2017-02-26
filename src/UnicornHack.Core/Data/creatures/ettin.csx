new Creature
{
    Name = "ettin",
    Species = Species.Giant,
    ArmorClass = 3,
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
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    InitialLevel = 10,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Boast
}
