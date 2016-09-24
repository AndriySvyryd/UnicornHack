new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 2,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
    Alignment = -3,
    Noise = ActorNoiseType.Speach,
    Name = "mountain centaur",
    Species = Species.Centaur,
    MovementRate = 20,
    Size = Size.Large,
    Weight = 2000,
    Nutrition = 800,
    SimpleProperties = new HashSet<string> { "Infravisibility", "HumanoidTorso", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
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
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
    }
}
