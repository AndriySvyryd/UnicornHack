new CreatureVariant
{
    InitialLevel = 1,
    ArmorClass = 10,
    MagicResistance = 5,
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.SmallGroup,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Speach,
    NextStageName = "gnome lord",
    Name = "gnome",
    Species = Species.Gnome,
    MovementRate = 6,
    Size = Size.Small,
    Weight = 650,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
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
