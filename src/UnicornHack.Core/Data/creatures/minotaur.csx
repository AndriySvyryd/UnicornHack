new CreatureVariant
{
    InitialLevel = 15,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.GemCollector | MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Roar,
    Name = "minotaur",
    Species = Species.Minotaur,
    MovementRate = 15,
    Size = Size.Large,
    Weight = 1500,
    Nutrition = 600,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 16 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 16 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Headbutt,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 9 } }
        }
    }
}
