namespace UnicornHack.Primitives;

public enum EffectType
{
    Default = 0,

    // Damage
    AcidDamage,
    BleedDamage,
    ToxinDamage,
    FireDamage,
    DrainEnergy,
    DrainLife,
    ColdDamage,
    LightDamage,
    PhysicalDamage,
    PsychicDamage,
    ShockDamage,
    WaterDamage,
    SonicDamage,
    Wither,

    // Status
    Bind,
    Blind,
    Cancel,
    Chill,
    Confuse,
    Corrode,
    Cripple,
    Daze,
    Deafen,
    Decay,
    Disarm,
    Doom,
    Enervate,
    Engulf,
    Expose,
    Fascinate,
    Frighten,
    Mute,
    Overcharge,
    Paralyze,
    Sedate,
    Slow,
    Stagger,
    Stick,
    Stun,
    Suffocate,
    Taunt,
    Topple,
    Wet,

    // Structural
    AddAbility,
    DecreaseAbilityLevel,
    ConferLycanthropy,
    DisableItem,
    EquipItem,
    Polymorph,
    RemoveItem,

    // Other
    Activate,
    ChangeProperty,
    GainXP,
    Heal,
    Recharge,
    Move,
    LevelTeleport,
    Teleport
}
