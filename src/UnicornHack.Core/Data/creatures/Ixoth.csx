new CreatureVariant
{
    InitialLevel = 16,
    ArmorClass = -1,
    MagicResistance = 20,
    GenerationFlags = GenerationFlags.NonGenocidable | GenerationFlags.NonPolymorphable,
    Behavior = MonsterBehavior.RangedPeaceful | MonsterBehavior.Stalking | MonsterBehavior.GoldCollector | MonsterBehavior.GemCollector | MonsterBehavior.Covetous,
    Alignment = -14,
    Noise = ActorNoiseType.Quest,
    Name = "Ixoth",
    Species = Species.Dragon,
    SpeciesClass = SpeciesClass.Reptile,
    MovementRate = 12,
    Size = Size.Gigantic,
    Weight = 4500,
    Nutrition = 1500,
    SimpleProperties = new HashSet<string> { "Flight", "FlightControl", "InvisibilityDetection", "Infravision", "AnimalBody", "Handlessness", "Maleness", "Carnivorism" },
    ValuedProperties = new Dictionary<string, Object> { { "FireResistance", 3 }, { "PoisonResistance", 3 }, { "StoningResistance", 3 }, { "DangerAwareness", 3 }, { "ThickHide", 3 } },
    Abilities = new List<Ability>
    {
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Breath,
            Timeout = 1,
            Effects = new AbilityEffect[] { new FireDamage { Damage = 27 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Bite,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 18 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Claw,
            Timeout = 1,
            Effects = new AbilityEffect[] { new PhysicalDamage { Damage = 10 } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Spell,
            Timeout = 1,
            Effects = new AbilityEffect[] { new ScriptedEffect { Script = "ArcaneSpell" } }
        }
,
        new Ability
        {
            Activation = AbilityActivation.Targetted,
            Action = AbilityAction.Touch,
            Timeout = 1,
            Effects = new AbilityEffect[] { new StealAmulet() }
        }
    }
}
