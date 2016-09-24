new CreatureVariant
{
    InitialLevel = 14,
    ArmorClass = -3,
    MagicResistance = 80,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector | MonsterBehavior.Covetous,
    Alignment = -10,
    CorpseVariantName = "",
    Name = "Vlad the Impaler",
    Species = Species.Vampire,
    SpeciesClass = SpeciesClass.ShapeChanger | SpeciesClass.Undead,
    MovementRate = 18,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Flight", "FlightControl", "Infravision", "Humanoidness", "Breathlessness", "Maleness" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "SicknessResistance", 3 }, { "Regeneration", 3 } },
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
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new DrainLife { Amount = 5 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new Infect { } } }
    }
}
