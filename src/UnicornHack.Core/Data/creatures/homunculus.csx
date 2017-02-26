new Creature
{
    Name = "homunculus",
    Species = Species.Homunculus,
    ArmorClass = 6,
    MagicResistance = 10,
    MovementDelay = 100,
    Weight = 60,
    Size = Size.Small,
    Nutrition = 60,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 1 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new Sleep { Duration = 2 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "SleepResistance", "Infravision", "Infravisibility", "Mindlessness", "Asexuality" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 }, { "Regeneration", 3 } },
    InitialLevel = 2,
    GenerationFrequency = Frequency.Sometimes,
    Behavior = MonsterBehavior.Stalking,
    Alignment = -7
}
