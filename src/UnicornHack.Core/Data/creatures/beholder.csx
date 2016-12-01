new CreatureVariant
{
    Name = "beholder",
    Species = Species.FloatingSphere,
    SpeciesClass = SpeciesClass.Aberration,
    InitialLevel = 8,
    ArmorClass = 4,
    MagicResistance = 35,
    MovementRate = 4,
    Weight = 250,
    Size = Size.Medium,
    Nutrition = 50,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Gaze,
            Timeout = 7,
            Effects = new AbilityEffect[] { new Disintegrate { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Gaze,
            Timeout = 7,
            Effects = new AbilityEffect[] { new Slow { Duration = 13 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Gaze,
            Timeout = 7,
            Effects = new AbilityEffect[] { new Sleep { Duration = 13 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Gaze,
            Timeout = 7,
            Effects = new AbilityEffect[] { new Confuse { Duration = 13 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Gaze, Timeout = 7, Effects = new AbilityEffect[] { new Stone() } },
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Gaze, Timeout = 7, Effects = new AbilityEffect[] { new Disenchant() } }
    }
,
    SimpleProperties = new HashSet<string>
    {
        "Flight",
        "FlightControl",
        "Infravision",
        "Infravisibility",
        "Breathlessness",
        "Limblessness",
        "Headlessness",
        "Asexuality",
        "NoInventory"
    }
,
    ValuedProperties = new Dictionary<string, Object> { { "ColdResistance", 3 }, { "DangerAwareness", 3 }, { "Stealthiness", 3 } },
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Wandering,
    Alignment = -10
}
