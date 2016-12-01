new CreatureVariant
{
    Name = "gnome king",
    Species = Species.Gnome,
    PreviousStageName = "gnome lord",
    InitialLevel = 5,
    ArmorClass = 10,
    MagicResistance = 20,
    MovementRate = 10,
    Weight = 750,
    Size = Size.Small,
    Nutrition = 300,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Maleness", "Omnivorism" },
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Speach
}
