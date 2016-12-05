new Creature
{
    Name = "housecat",
    Species = Species.Cat,
    SpeciesClass = SpeciesClass.Feline,
    ArmorClass = 5,
    MovementRate = 16,
    Weight = 200,
    Size = Size.Small,
    Nutrition = 150,
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
    PreviousStageName = "kitten",
    NextStageName = "large cat",
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Mew
}
