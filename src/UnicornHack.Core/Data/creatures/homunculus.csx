new CreatureVariant
{
    InitialLevel = 2,
    ArmorClass = 6,
    MagicResistance = 10,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Stalking,
    Alignment = -7,
    Name = "homunculus",
    Species = Species.Homunculus,
    MovementRate = 12,
    Size = Size.Small,
    Weight = 60,
    Nutrition = 60,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Infravisibility", "Mindlessness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Sleep { Duration = 2 } }
        }
    }
}
