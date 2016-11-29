new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 10,
    GenerationFlags = GenerationFlags.Entourage,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.GoldCollector | MonsterBehavior.WeaponCollector,
    Alignment = -5,
    Noise = ActorNoiseType.Grunt,
    PreviousStageName = "orc",
    Name = "orc captain",
    Species = Species.Orc,
    MovementRate = 9,
    Size = Size.Medium,
    Weight = 1350,
    Nutrition = 350,
    SimpleProperties = new HashSet<string> { "Infravision", "Infravisibility", "Humanoidness", "Omnivorism" },
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
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
    }
}
