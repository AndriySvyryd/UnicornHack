new CreatureVariant
{
    Name = "dog",
    Species = Species.Dog,
    SpeciesClass = SpeciesClass.Canine,
    PreviousStageName = "little dog",
    NextStageName = "large dog",
    InitialLevel = 4,
    ArmorClass = 5,
    MovementRate = 16,
    Weight = 400,
    Size = Size.Medium,
    Nutrition = 300,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Bark
}
