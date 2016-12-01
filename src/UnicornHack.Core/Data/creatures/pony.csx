new CreatureVariant
{
    Name = "pony",
    Species = Species.Horse,
    SpeciesClass = SpeciesClass.Quadrupedal,
    NextStageName = "horse",
    InitialLevel = 3,
    ArmorClass = 6,
    MovementRate = 16,
    Weight = 1300,
    Size = Size.Medium,
    Nutrition = 900,
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
,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Herbivorism", "SingularInventory" },
    GenerationFrequency = Frequency.Often,
    Behavior = MonsterBehavior.Domesticable | MonsterBehavior.Mountable | MonsterBehavior.Wandering,
    Noise = ActorNoiseType.Neigh
}
