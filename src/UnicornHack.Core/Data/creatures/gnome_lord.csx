new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 10,
    MagicResistance = 5,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Speach,
    PreviousStageName = "gnome",
    NextStageName = "gnome king",
    Name = "gnome lord",
    Species = Species.Gnome,
    MovementRate = 8,
    Size = Size.Small,
    Weight = 700,
    Nutrition = 250,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Maleness", "Omnivorism" },
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
