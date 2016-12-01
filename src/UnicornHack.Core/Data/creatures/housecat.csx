new CreatureVariant
{
    Name = "housecat",
    Species = Species.Cat,
    SpeciesClass = SpeciesClass.Feline,
    PreviousStageName = "kitten",
    NextStageName = "large cat",
    InitialLevel = 4,
    ArmorClass = 5,
    MovementRate = 16,
    Weight = 200,
    Size = Size.Small,
    Nutrition = 150,
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
    Noise = ActorNoiseType.Mew
}
