new CreatureVariant
{
    Name = "large cat",
    Species = Species.Cat,
    SpeciesClass = SpeciesClass.Feline,
    PreviousStageName = "housecat",
    InitialLevel = 6,
    ArmorClass = 4,
    MovementRate = 15,
    Weight = 250,
    Size = Size.Small,
    Nutrition = 200,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Bark
}
