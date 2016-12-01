new CreatureVariant
{
    Name = "kitten",
    Species = Species.Cat,
    SpeciesClass = SpeciesClass.Feline,
    NextStageName = "housecat",
    InitialLevel = 2,
    ArmorClass = 6,
    MovementRate = 18,
    Weight = 150,
    Size = Size.Small,
    Nutrition = 100,
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
