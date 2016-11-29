new CreatureVariant
{
    InitialLevel = 16,
    ArmorClass = 3,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.Covetous,
    Alignment = -15,
    Noise = ActorNoiseType.Quest,
    Name = "Scorpius",
    Species = Species.Scorpion,
    SpeciesClass = SpeciesClass.Vermin,
    MovementRate = 12,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Handlessness", "Maleness", "Carnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 }, { "StoningResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Claw, Timeout = 1, Effects = new AbilityEffect[] { new StealAmulet() } },
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Infect { Strength = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new AbilityEffect[] { new PoisonDamage { Damage = 7 } } }
    }
}
