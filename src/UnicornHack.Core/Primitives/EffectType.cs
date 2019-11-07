namespace UnicornHack.Primitives
{
    public enum EffectType
    {
        Default = 0,
        Activate,
        AddAbility,
        AddDuplicateAbility,
        Bind,
        Bleed,
        Blight,
        Blind,
        Burn,
        ChangeProperty,
        ChangeRace,
        ConferLycanthropy,
        Confuse,
        Corrode,
        Cripple,
        Curse,
        Deafen,
        DisableItem,
        Disarm,
        DrainEnergy,
        DrainLife,
        Engulf,
        Freeze,
        GainXP,
        Heal,
        LevelTeleport,
        LightDamage,
        Move,
        Paralyze,
        PhysicalDamage,
        PsychicDamage,
        Recharge,
        RemoveItem,
        Sedate,
        Shock,
        Slime,
        Slow,
        Soak,
        SonicDamage,
        Stick,
        Stone,
        Stun,
        Suffocate,
        Teleport,
        Wither
    }

    public static class EffectTypeExtensions
    {
        public static bool IsDamage(this EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.Burn:
                case EffectType.Bleed:
                case EffectType.Blight:
                case EffectType.Corrode:
                case EffectType.Freeze:
                case EffectType.LightDamage:
                case EffectType.PsychicDamage:
                case EffectType.Shock:
                case EffectType.Soak:
                case EffectType.SonicDamage:
                case EffectType.Wither:
                case EffectType.PhysicalDamage:
                case EffectType.DrainLife:
                    return true;
                default:
                    return false;
            }
        }
    }
}
