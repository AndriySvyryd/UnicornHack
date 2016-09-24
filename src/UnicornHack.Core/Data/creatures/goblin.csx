new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 10,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.WeaponCollector,
    Alignment = -3,
    Noise = ActorNoiseType.Grunt,
    Name = "goblin",
    Species = Species.Goblin,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 400,
    Nutrition = 100,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
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
