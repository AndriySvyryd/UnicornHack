namespace UnicornHack.Services.LogEvents;

public readonly struct AttackEvent
{
    public AttackEvent(
        GameEntity sensorEntity, GameEntity attackerEntity, GameEntity? victimEntity,
        SenseType attackerSensed, SenseType victimSensed, IReadOnlyList<GameEntity> appliedEffects,
        AbilityAction abilityAction, GameEntity? weaponEntity, bool ranged, bool hit)
    {
        SensorEntity = sensorEntity;
        AttackerEntity = attackerEntity;
        VictimEntity = victimEntity;
        AttackerSensed = attackerSensed;
        VictimSensed = victimSensed;
        AppliedEffects = appliedEffects;
        AbilityAction = abilityAction;
        WeaponEntity = weaponEntity;
        Ranged = ranged;
        Hit = hit;
    }

    public GameEntity SensorEntity
    {
        get;
    }

    public GameEntity AttackerEntity
    {
        get;
    }

    public GameEntity? VictimEntity
    {
        get;
    }

    public SenseType AttackerSensed
    {
        get;
    }

    public SenseType VictimSensed
    {
        get;
    }

    public IReadOnlyList<GameEntity> AppliedEffects
    {
        get;
    }

    public AbilityAction AbilityAction
    {
        get;
    }

    public GameEntity? WeaponEntity
    {
        get;
    }

    public bool Ranged
    {
        get;
    }

    public bool Hit
    {
        get;
    }
}
