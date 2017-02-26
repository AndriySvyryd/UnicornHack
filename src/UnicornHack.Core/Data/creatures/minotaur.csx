new Creature
{
    Name = "minotaur",
    Species = Species.Minotaur,
    ArmorClass = 6,
    MovementDelay = 80,
    Weight = 1500,
    Size = Size.Large,
    Nutrition = 600,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 16 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 16 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 9 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    InitialLevel = 15,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Roar
}
