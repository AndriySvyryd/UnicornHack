new CreatureVariant
{
    Name = "doctor",
    Species = Species.Human,
    InitialLevel = 11,
    MovementRate = 6,
    Weight = 1000,
    Size = Size.Medium,
    Nutrition = 400,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new Heal { Amount = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Infravisibility", "Humanoidness", "Omnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "PoisonResistance", 3 } },
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.RangedPeaceful,
    Noise = ActorNoiseType.Doctor
}
