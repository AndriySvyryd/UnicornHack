namespace UnicornHack.Systems.Effects
{
    public enum EffectType
    {
        Default = 0,
        Activate,
        AddAbility,
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
        Disarm,
        Disintegrate,
        DrainEnergy,
        DrainLife,
        Engulf,
        Freeze,
        GainXP,
        Heal,
        LevelTeleport,
        MagicalDamage,
        Move,
        Paralyze,
        PhysicalDamage,
        RemoveItem,
        Sedate,
        Shock,
        Slime,
        Slow,
        Soak,
        StealGold,
        StealItem,
        Stick,
        Stone,
        Stun,
        Suffocate,
        Teleport
    }

    public static class EffectTypeExtentions
    {
        public static bool IsDamage(this EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.Burn:
                case EffectType.Corrode:
                case EffectType.Disintegrate:
                case EffectType.Blight:
                case EffectType.Freeze:
                case EffectType.Bleed:
                case EffectType.Shock:
                case EffectType.Soak:
                case EffectType.MagicalDamage:
                case EffectType.PhysicalDamage:
                    return true;
                default:
                    return false;
            }
        }
    }
}
