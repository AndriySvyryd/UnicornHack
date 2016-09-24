new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 10,
    MagicResistance = 20,
    GenerationFlags = GenerationFlags.NonPolymorphable | GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Uncommonly,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
    Noise = ActorNoiseType.Speach,
    PreviousStageName = "gnome lord",
    Name = "gnome king",
    Species = Species.Gnome,
    MovementRate = 10,
    Size = Size.Small,
    Weight = 750,
    Nutrition = 300,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Maleness", "Omnivorism" },
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
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}
