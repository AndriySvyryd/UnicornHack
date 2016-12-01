new CreatureVariant
{
    Name = "warg",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine,
    InitialLevel = 8,
    ArmorClass = 3,
    MovementRate = 12,
    Weight = 1400,
    Size = Size.Large,
    Nutrition = 600,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.Mountable,
    Alignment = -5,
    Noise = ActorNoiseType.Bark
}
