new CreatureVariant
{
    Name = "glass piercer",
    Species = Species.Piercer,
    InitialLevel = 7,
    ArmorClass = 1,
    MovementRate = 1,
    Weight = 400,
    Size = Size.Small,
    Nutrition = 200,
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
,
    SimpleProperties = new HashSet<string> { "Camouflage", "Eyelessness", "Limblessness", "Clinginess", "Carnivorism", "NoInventory" },
    ValuedProperties = new Dictionary<string, Object> { { "AcidResistance", 3 }, { "Stealthiness", 3 } },
    GenerationFrequency = Frequency.Occasionally
}
