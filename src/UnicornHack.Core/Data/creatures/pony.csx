new CreatureVariant
{
    InitialLevel = 3,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Mountable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Neigh,
    NextStageName = "horse",
    Name = "pony",
    Species = Species.Horse,
    SpeciesClass = SpeciesClass.Quadrupedal,
    MovementRate = 16,
    Size = Size.Medium,
    Weight = 1300,
    Nutrition = 900,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Kick,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 3 } }
        }
    }
}
