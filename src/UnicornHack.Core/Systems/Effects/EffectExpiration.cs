namespace UnicornHack.Systems.Effects;

public readonly struct EffectExpiration(EffectDuration? expirationEvent, int? target = null)
{
    public EffectDuration? Event { get; } = expirationEvent;
    public int? Target { get; } = target;
}