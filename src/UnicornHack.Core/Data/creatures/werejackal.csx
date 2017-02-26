new Creature
{
    Name = "werejackal",
    Species = Species.Human,
    SpeciesClass = SpeciesClass.ShapeChanger,
    ArmorClass = 10,
    MagicResistance = 10,
    MovementDelay = 100,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnMeleeAttack,
            Action = AbilityAction.Modifier,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 5,
            Effects = new HashSet<Effect> { new ConferLycanthropy { VariantName = "jackalwere" } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new ConferLycanthropy { VariantName = "jackalwere" } } }
    }
,
    SimpleProperties = new HashSet<string> { "Infravisibility", "Humanoidness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 }, { "Lycanthropy", "jackalwere" } },
    InitialLevel = 2,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Rarely,
    Behavior = MonsterBehavior.WeaponCollector,
    Alignment = -7,
    Noise = ActorNoiseType.Lycanthrope
}
