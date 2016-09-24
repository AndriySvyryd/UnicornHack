new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = 4,
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Mountable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Neigh,
    PreviousStageName = "horse",
    Name = "warhorse",
    Species = Species.Horse,
    SpeciesClass = SpeciesClass.Quadrupedal,
    MovementRate = 24,
    Size = Size.Large,
    Weight = 1800,
    Nutrition = 1300,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
    }
}
