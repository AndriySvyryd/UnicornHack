new Creature
{
    Name = "glass piercer",
    Species = Species.Piercer,
    ArmorClass = 1,
    MovementDelay = 1200,
    Weight = 400,
    Size = Size.Small,
    Nutrition = 200,
    Abilities = new HashSet<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 14 } }
        }
    }
,
    SimpleProperties = new HashSet<string> { "Camouflage", "Eyelessness", "Limblessness", "Clinginess", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "AcidResistance", 3 }, { "Stealthiness", 3 } },
    InitialLevel = 7,
    GenerationFrequency = Frequency.Occasionally
}
