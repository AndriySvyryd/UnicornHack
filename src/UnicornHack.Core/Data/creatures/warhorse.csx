new CreatureVariant
{
    Name = "warhorse",
    Species = Species.Horse,
    SpeciesClass = SpeciesClass.Quadrupedal,
    PreviousStageName = "horse",
    InitialLevel = 7,
    ArmorClass = 4,
    MovementRate = 24,
    Weight = 1800,
    Size = Size.Large,
    Nutrition = 1300,
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Mountable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Neigh
}
