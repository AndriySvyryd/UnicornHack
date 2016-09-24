new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 3,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
    Alignment = -1,
    Noise = ActorNoiseType.Speach,
    Name = "forest centaur",
    Species = Species.Centaur,
    MovementRate = 18,
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
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
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
}
