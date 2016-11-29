new CreatureVariant
{
    InitialLevel = 7,
    ArmorClass = 1,
    GenerationFrequency = Frequency.Occasionally,
    Name = "glass piercer",
    Species = Species.Piercer,
    MovementRate = 1,
    Size = Size.Small,
    Weight = 400,
    Nutrition = 200,
    SimpleProperties = new HashSet<string> { "Camouflage", "Eyelessness", "Limblessness", "Clinginess", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "AcidResistance", 3 }, { "Stealthiness", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.OnTarget,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 14 } }
        }
    }
}
