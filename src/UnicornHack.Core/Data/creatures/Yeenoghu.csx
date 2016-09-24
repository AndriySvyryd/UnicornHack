new CreatureVariant
{
    InitialLevel = 56,
    ArmorClass = -5,
    MagicResistance = 80,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable | GenerationFlags.HellOnly,
    Behavior = MonsterBehavior.Stalking | MonsterBehavior.WeaponCollector | MonsterBehavior.MagicUser | MonsterBehavior.Covetous,
    Alignment = -15,
    Noise = ActorNoiseType.Gurgle,
    CorpseVariantName = "",
    Name = "Yeenoghu",
    Species = Species.DemonMajor,
    SpeciesClass = SpeciesClass.Demon,
    MovementRate = 18,
    Size = Size.Large,
    Weight = 1500,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Infravision", "InvisibilityDetection", "Infravisibility", "Maleness" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 }, { "SicknessResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
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
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Confuse { Duration = 9 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Paralyze { Duration = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new MagicalDamage { Damage = 7 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 10 } } }
    }
}
