new Creature
{
    Name = "mountain centaur",
    Species = Species.Centaur,
    ArmorClass = 2,
    MagicResistance = 10,
    MovementDelay = 60,
    Weight = 2000,
    Size = Size.Large,
    Nutrition = 800,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
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
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 4 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravisibility", "HumanoidTorso", "Omnivorism" },
    InitialLevel = 6,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
    Alignment = -3,
    Noise = ActorNoiseType.Speach
}
