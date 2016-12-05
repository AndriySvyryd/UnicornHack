new Creature
{
    Name = "gnome",
    Species = Species.Gnome,
    ArmorClass = 10,
    MagicResistance = 5,
    MovementRate = 6,
    Weight = 650,
    Size = Size.Small,
    Nutrition = 200,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
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
    InitialLevel = 1,
    NextStageName = "gnome lord",
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Speach
}
