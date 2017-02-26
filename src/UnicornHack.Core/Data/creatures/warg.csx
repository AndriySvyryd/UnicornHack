new Creature
{
    Name = "warg",
    Species = Species.Wolf,
    SpeciesClass = SpeciesClass.Canine,
    ArmorClass = 3,
    MovementDelay = 100,
    Weight = 1400,
    Size = Size.Large,
    Nutrition = 600,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 7 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    InitialLevel = 8,
    GenerationFrequency = Frequency.Commonly,
    Behavior = MonsterBehavior.Mountable,
    Alignment = -5,
    Noise = ActorNoiseType.Bark
}
