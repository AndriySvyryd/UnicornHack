new CreatureVariant
{
    InitialLevel = 8,
    ArmorClass = 3,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.Mountable,
    Alignment = -5,
    Noise = ActorNoiseType.Bark,
    Name = "warg",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine,
    MovementRate = 12,
    Size = Size.Large,
    Weight = 1400,
    Nutrition = 600,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
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
}
