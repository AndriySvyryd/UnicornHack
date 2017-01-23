new Creature
{
    Name = "Scorpius",
    Species = Species.Scorpion,
    SpeciesClass = SpeciesClass.Vermin,
    ArmorClass = 3,
    MovementRate = 12,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        },
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Sting,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Infect { Strength = 2 } }
        }
,
        new Ability { Activation = AbilityActivation.OnConsumption, Effects = new HashSet<Effect> { new PoisonDamage { Damage = 7 } } }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Handlessness", "Maleness", "Carnivorism", "StoningResistance" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "VenomResistance", 3 } },
    InitialLevel = 16,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.Covetous,
    Alignment = -15,
    Noise = ActorNoiseType.Quest
}
