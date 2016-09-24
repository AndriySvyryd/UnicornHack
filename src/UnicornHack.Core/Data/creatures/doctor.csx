new CreatureVariant
{
    InitialLevel = 11,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.RangedPeaceful,
    Noise = ActorNoiseType.Doctor,
    Name = "doctor",
    Species = Species.Human,
    MovementRate = 6,
    Size = Size.Medium,
    Weight = 1000,
    Nutrition = 400,
    SimpleProperties = new HashSet<string> { "Infravisibility", "Humanoidness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Heal { Amount = 7 } }
        }
    }
}
