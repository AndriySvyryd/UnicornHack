new Creature
{
    Name = "beholder",
    Species = Species.FloatingSphere,
    SpeciesClass = SpeciesClass.Aberration,
    ArmorClass = 4,
    MagicResistance = 35,
    MovementDelay = 300,
    Weight = 250,
    Size = Size.Medium,
    Nutrition = 50,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Gaze,
            Timeout = 7,
            Effects = new HashSet<Effect> { new Disintegrate { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Gaze,
            Timeout = 7,
            Effects = new HashSet<Effect> { new Slow { Duration = 13 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Gaze,
            Timeout = 7,
            Effects = new HashSet<Effect> { new Sleep { Duration = 13 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Gaze,
            Timeout = 7,
            Effects = new HashSet<Effect> { new Confuse { Duration = 13 } }
        }
,
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Gaze, Timeout = 7, Effects = new HashSet<Effect> { new Stone { } } },
        new Ability { Activation = AbilityActivation.OnTarget, Action = AbilityAction.Gaze, Timeout = 7, Effects = new HashSet<Effect> { new Disenchant { } } }
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
    InitialLevel = 8,
    GenerationFlags = GenerationFlags.NonPolymorphable,
    GenerationFrequency = Frequency.Occasionally,
    Behavior = MonsterBehavior.Wandering,
    Alignment = -10
}
