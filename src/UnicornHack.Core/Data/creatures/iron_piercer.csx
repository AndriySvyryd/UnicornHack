new Creature
{
    Name = "iron piercer",
    Species = Species.Piercer,
    ArmorClass = 2,
    MovementDelay = 1200,
    Weight = 300,
    Size = Size.Small,
    Nutrition = 150,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 10 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Camouflage", "Eyelessness", "Limblessness", "Clinginess", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "Stealthiness", 3 } },
    InitialLevel = 5,
    GenerationFrequency = Frequency.Occasionally
}
