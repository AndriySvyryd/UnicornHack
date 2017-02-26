new Creature
{
    Name = "dog",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    ArmorClass = 5,
    MovementDelay = 75,
    Weight = 400,
    Size = Size.Medium,
    Nutrition = 300,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    InitialLevel = 4,
    PreviousStageName = "little dog",
    NextStageName = "large dog",
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Bark
}
