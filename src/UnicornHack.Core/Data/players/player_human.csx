new Player
{
    Name = "player human",
    BaseName = "human",
    MovementRate = 14,
    SimpleProperties = new HashSet<string> { "Infravisibility", "Humanoidness", "Omnivorism" },
    Strength = 12,
    Dexterity = 12,
    Constitution = 12,
    Intelligence = 12,
    Willpower = 12,
    Speed = 12,
    DefaultAttack = new Ability
    {
        Activation = AbilityActivation.OnTarget,
        Action = AbilityAction.Punch,
        ActionPointCost = 100,
        Effects = new HashSet<Effect> { new PhysicalDamage { Damage = 4 } }
    }
}
