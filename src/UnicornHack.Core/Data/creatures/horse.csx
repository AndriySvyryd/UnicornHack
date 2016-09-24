new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 5,
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Mountable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Neigh,
    PreviousStageName = "pony",
    NextStageName = "warhorse",
    Name = "horse",
    Species = Species.Horse,
    SpeciesClass = SpeciesClass.Quadrupedal,
    MovementRate = 20,
    Size = Size.Large,
    Weight = 1500,
    Nutrition = 1100,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 4 } }
        }
    }
}
