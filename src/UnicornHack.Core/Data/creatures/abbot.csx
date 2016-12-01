new CreatureVariant
{
    Name = "abbot",
    Species = Species.Human,
    NextStageName = "monk",
    InitialLevel = 5,
    ArmorClass = 6,
    MagicResistance = 10,
    MovementRate = 12,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Punch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravisibility", "Humanoidness", "Herbivorism" },
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.Peaceful,
    Alignment = 3,
    Noise = ActorNoiseType.Speach
}
