new Creature
{
    Name = "warhorse",
    Species = Species.Horse,
    SpeciesClass = SpeciesClass.Quadrupedal,
    ArmorClass = 4,
    MovementRate = 24,
    Weight = 1800,
    Size = Size.Large,
    Nutrition = 1300,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 5 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    InitialLevel = 7,
    PreviousStageName = "horse",
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Mountable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Neigh
}
