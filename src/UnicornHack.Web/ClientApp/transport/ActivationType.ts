export const enum ActivationType {
    Default = 0,
    Always = 1 << 0,
    WhileToggled = 1 << 1,
    WhilePossessed = 1 << 2,
    WhileEquipped = 1 << 3,
    WhileAboveLevel = 1 << 4,
    ManualActivation = 1 << 5,
    Targeted = 1 << 6,
    OnMeleeAttack = 1 << 7,
    OnRangedAttack = 1 << 8,
    OnMeleeHit = 1 << 10,
    OnRangedHit = 1 << 11,
    OnDeath = 1 << 14,
    OnDigestion = 1 << 15,

    Continuous = Always | WhileToggled | WhilePossessed | WhileEquipped | WhileAboveLevel,
    Slottable = WhileToggled | ManualActivation | Targeted,
    OnAttack = OnMeleeAttack | OnRangedAttack,
    OnHit = OnMeleeHit | OnRangedHit
}