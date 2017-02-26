new Creature
{
    Name = "horse",
    Species = Species.Horse,
    SpeciesClass = SpeciesClass.Quadrupedal,
    ArmorClass = 5,
    MovementDelay = 60,
    Weight = 1500,
    Size = Size.Large,
    Nutrition = 1100,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 4 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    InitialLevel = 5,
    PreviousStageName = "pony",
    NextStageName = "warhorse",
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Mountable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Neigh
}
