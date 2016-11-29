new CreatureVariant
{
    InitialLevel = 5,
    ArmorClass = 6,
    GenerationFrequency = Frequency.Commonly,
    Noise = ActorNoiseType.Growl,
    Name = "lynx",
    Species = Species.Cat,
    SpeciesClass = SpeciesClass.Feline,
    MovementRate = 15,
    Size = Size.Medium,
    Weight = 400,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "AnimalBody", "Infravisibility", "Handlessness", "Carnivorism", "SingularInventory" },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 5 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 2 } }
        }
    }
}
