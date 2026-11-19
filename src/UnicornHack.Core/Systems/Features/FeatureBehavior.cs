namespace UnicornHack.Systems.Features;

[Flags]
public enum FeatureBehavior
{
    None = 0,
    BlocksMovement = 1 << 0,
    AllowDestroy = 1 << 1,
    TriggerOnEntry = 1 << 2,
    TriggerOnInterval = 1 << 3
}
