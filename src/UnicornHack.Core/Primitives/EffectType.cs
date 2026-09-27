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

    // Buffs

    // Debuffs
    Bind = 40,
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
    Weaken,

    // Structural
    AddAbility = 80,
    DecreaseAbilityLevel,
    ConferLycanthropy,
    DisableItem,
    EquipItem,
    Polymorph,
    RemoveItem,

    // Other
    Activate = 90,
    ChangeProperty,
    GainXP,
    Heal,
    Recharge,
    Move,
    LevelTeleport,
    Teleport
}
