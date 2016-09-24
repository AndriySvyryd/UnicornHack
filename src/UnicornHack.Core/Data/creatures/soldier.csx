new CreatureVariant
{
    InitialLevel = 6,
    ArmorClass = 10,
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.Wandering | MonsterBehavior.Stalking | MonsterBehavior.Displacing | MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector | MonsterBehavior.Bribeable,
    Alignment = -2,
    Noise = ActorNoiseType.Soldier,
    NextStageName = "sergeant",
    Name = "soldier",
    Species = Species.Human,
    MovementRate = 10,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Infravisibility", "Humanoidness", "Omnivorism" },
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
    }
}
