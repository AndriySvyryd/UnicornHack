using System.Buffers;
using UnicornHack.Services;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

public class PlayerChangeBuilder : IEntityChangeListener<GameEntity>
{
    private const uint NextActionTickDirty = 1u << 5;
    private const uint XpDirty = 1u << 10;
    private const uint HpDirty = 1u << 11;
    private const uint MaxHpDirty = 1u << 12;
    private const uint EpDirty = 1u << 13;
    private const uint MaxEpDirty = 1u << 14;
    private const uint ReservedEpDirty = 1u << 15;
    private const uint FortuneDirty = 1u << 16;
    private const uint PendingOptionsDirty = 1u << 17;

    private struct PendingScalarChanges
    {
        public uint DirtyMask;
        public int NextActionTick;
        public int XP;
        public int Hp;
        public int MaxHp;
        public int Ep;
        public int MaxEp;
        public int ReservedEp;
        public int Fortune;
    }

    private readonly int _playerEntityId;
    private readonly AbilityChangeBuilder _abilityBuilder;
    private readonly EffectChangeBuilder _effectBuilder;
    private readonly LogChangeBuilder _logBuilder;
    private readonly RoomChangeBuilder _roomChangeBuilder;

    private PendingScalarChanges _pendingScalars;

    public int PlayerEntityId => _playerEntityId;

    public int LastSentTick
    {
        get; set;
    }

    /// <summary>
    ///     Id of the level entity the client last received in a full snapshot.
    ///     Used to detect cross-level transitions so a full <see cref="RoomChange" /> snapshot
    ///     is emitted instead of a delta (which carries only entity/terrain changes, not
    ///     <see cref="RoomChange.Width" /> / <see cref="RoomChange.Height" /> /
    ///     <see cref="RoomChange.BranchName" /> / <see cref="RoomChange.Depth" />).
    /// </summary>
    private int LastSentRoomId
    {
        get; set;
    }

    public PlayerChangeBuilder(GameEntity playerEntity, RoomChangeBuilder roomChangeBuilder)
    {
        _playerEntityId = playerEntity.Id;
        LastSentTick = playerEntity.Game.CurrentTick;
        LastSentRoomId = playerEntity.Position?.RoomId ?? 0;
        _roomChangeBuilder = roomChangeBuilder;

        _abilityBuilder = new AbilityChangeBuilder(playerEntity.Id);
        _abilityBuilder.ResetProjection(playerEntity);
        _abilityBuilder.ResetAccumulatorPlaceholders(playerEntity);
        _effectBuilder = new EffectChangeBuilder(playerEntity.Id);
        _logBuilder = new LogChangeBuilder(playerEntity);
    }

    public bool HasObservableChanges(GameManager manager)
    {
        var playerEntity = manager.FindEntity(_playerEntityId);
        var position = playerEntity?.Position;
        var hasAbilityChanges = _abilityBuilder.HasChanges || _abilityBuilder.HasAccumulatorChanges;
        var hasEffectChanges = _effectBuilder.HasChanges;

        // The room the player occupies is tracked so the client keeps the actor list up to date.
        var currentRoom = position?.RoomEntity.Room;
        var hasRoomChanges = false;
        if (playerEntity != null && currentRoom != null)
        {
            var context = new SerializationContext(null, playerEntity, manager.Game.Services);
            hasRoomChanges = _roomChangeBuilder.HasChanges(currentRoom, in context);
        }

        if (hasRoomChanges || HasScalarChanges() || hasAbilityChanges || hasEffectChanges)
        {
            return true;
        }

        if (_logBuilder.HasChanges)
        {
            return true;
        }

        // A move to a different room is a change even when nothing else is; no position maps to room 0.
        var currentRoomId = position?.RoomId ?? 0;
        return currentRoomId != LastSentRoomId;
    }

    public PlayerChange BuildChangeSet(GameManager manager, GameServices services)
    {
        var playerEntity = manager.FindEntity(_playerEntityId);
        if (playerEntity == null)
        {
            Debug.Assert(false, $"Player entity with id {_playerEntityId} not found");

            return new PlayerChange
            {
                PreviousTick = LastSentTick,
                CurrentTick = manager.Game.CurrentTick
            };
        }

        var change = new PlayerChange
        {
            PreviousTick = LastSentTick,
            CurrentTick = manager.Game.CurrentTick
        };
        LastSentTick = manager.Game.CurrentTick;

        var position = playerEntity.Position;
        var currentRoomEntity = position?.RoomEntity;
        var currentRoomId = currentRoomEntity?.Id ?? 0;
        var context = new SerializationContext(null, playerEntity, services);
        if (currentRoomEntity == null)
        {
            // The player has no position at all (e.g. being torn down); there is no room to send.
            LastSentRoomId = 0;
        }
        else if (currentRoomId != LastSentRoomId)
        {
            // Player moved to a different level. Emit a full snapshot.
            change.Room = RoomChangeBuilder.SerializeRoom(
                currentRoomEntity, in context, _roomChangeBuilder);
            _roomChangeBuilder.ResetRoomBaseline(playerEntity);
            LastSentRoomId = currentRoomId;
        }
        else
        {
            var levelChangeSet = _roomChangeBuilder.GetSerializedRoom(currentRoomEntity, in context);
            if (levelChangeSet != null)
            {
                change.Room = levelChangeSet;
            }

        }

        if (_abilityBuilder.HasChanges || _abilityBuilder.HasAccumulatorChanges)
        {
            var serializedAbilities = _abilityBuilder.BuildChanges(in context, playerEntity);
            if (serializedAbilities != null)
            {
                change.Abilities = serializedAbilities;
            }
        }

        if (_effectBuilder.HasChanges)
        {
            change.Effects = _effectBuilder.BuildChanges();
        }

        var serializedLog = _logBuilder.GetSerializedLogEntries();
        if (serializedLog != null)
        {
            change.Log = serializedLog;
        }

        var pendingMask = _pendingScalars.DirtyMask;
        if ((pendingMask & NextActionTickDirty) != 0)
        {
            change.NextActionTick = _pendingScalars.NextActionTick;
        }

        if ((pendingMask & XpDirty) != 0)
        {
            change.XP = _pendingScalars.XP;
        }

        if ((pendingMask & HpDirty) != 0)
        {
            change.Hp = _pendingScalars.Hp;
        }

        if ((pendingMask & MaxHpDirty) != 0)
        {
            change.MaxHp = _pendingScalars.MaxHp;
        }

        if ((pendingMask & EpDirty) != 0)
        {
            change.Ep = _pendingScalars.Ep;
        }

        if ((pendingMask & MaxEpDirty) != 0)
        {
            change.MaxEp = _pendingScalars.MaxEp;
        }

        if ((pendingMask & ReservedEpDirty) != 0)
        {
            change.ReservedEp = _pendingScalars.ReservedEp;
        }

        if ((pendingMask & FortuneDirty) != 0)
        {
            change.Fortune = _pendingScalars.Fortune;
        }

        if ((pendingMask & PendingOptionsDirty) != 0)
        {
            change.PendingOptions = SerializePlayerOptions(playerEntity);
        }

        return change;
    }

    public void Reset(GameEntity playerEntity)
    {
        _pendingScalars = default;
        _abilityBuilder.Reset();
        _abilityBuilder.ResetAccumulatorChanges();
        _effectBuilder.Reset();

        LastSentRoomId = playerEntity.Position?.RoomId ?? 0;
        _logBuilder.Reset();
    }

    internal void ResetProjection(GameEntity playerEntity)
    {
        Reset(playerEntity);
        _abilityBuilder.ResetProjection(playerEntity);
        _abilityBuilder.ResetAccumulatorPlaceholders(playerEntity);
    }

    public static PlayerChange SerializePlayer(
        GameEntity playerEntity, in SerializationContext context)
    {
        var player = playerEntity.Player!;
        var being = playerEntity.Being!;

        var position = playerEntity.Position;
        var onMap = position is { Cell: not null };
        var serializedLevel = position == null
            ? null
            : RoomChangeBuilder.SerializeRoom(position.RoomEntity, in context);

        var visibleAbilityCount = 0;
        foreach (var abilityEntity in playerEntity.ActiveEffects!.AvailableAbilities)
        {
            var ability = abilityEntity.Ability!;
            if (!ability.IsLeveledSupplement && AbilityChangeBuilder.IsClientVisible(ability))
            {
                visibleAbilityCount++;
            }
        }

        var placeholders = AbilityChangeBuilder.GetAccumulatorPlaceholders(playerEntity);
        var abilities = new Dictionary<int, AbilityChange>(visibleAbilityCount + placeholders.Count);
        foreach (var abilityEntity in playerEntity.ActiveEffects!.AvailableAbilities)
        {
            var ability = abilityEntity.Ability!;
            if (!ability.IsLeveledSupplement && AbilityChangeBuilder.IsClientVisible(ability))
            {
                abilities.Add(
                    abilityEntity.Id,
                    AbilityChangeBuilder.SerializeAbility(abilityEntity, context.Services));
            }
        }

        foreach (var (id, placeholder) in placeholders)
        {
            abilities.Add(id, placeholder);
        }

        var logCount = player.LogEntries.Count;
        var serializedLogCount = Math.Min(logCount, 10);
        var logEntries = new Dictionary<int, LogEntryChange>(serializedLogCount);
        if (logCount != 0)
        {
            var logBuffer = ArrayPool<LogEntry>.Shared.Rent(logCount);
            var index = 0;
            try
            {
                foreach (var logEntry in player.LogEntries)
                {
                    logBuffer[index++] = logEntry;
                }

                Array.Sort(logBuffer, 0, logCount, LogEntry.Comparer);
                for (var i = logCount - serializedLogCount; i < logCount; i++)
                {
                    var logEntry = logBuffer[i];
                    logEntries.Add(logEntry.Id, LogChangeBuilder.SerializeLogEntry(logEntry));
                }
            }
            finally
            {
                Array.Clear(logBuffer, 0, logCount);
                ArrayPool<LogEntry>.Shared.Return(logBuffer);
            }
        }

        return new PlayerChange
        {
            ChangedProperties = null,
            Name = player.ProperName,
            Id = onMap ? position!.Knowledge?.Id ?? -1 : -1,
            PreviousTick = 0,
            CurrentTick = player.Game.CurrentTick,
            Room = serializedLevel,
            Abilities = abilities,
            Effects = EffectChangeBuilder.SerializeSnapshot(playerEntity),
            Log = logEntries,
            NextActionTick = player.NextActionTick ?? 0,
            XP = being.ExperiencePoints,
            Hp = being.HitPoints,
            MaxHp = being.HitPointMaximum,
            Ep = being.EnergyPoints,
            MaxEp = being.EnergyPointMaximum,
            ReservedEp = being.ReservedEnergyPoints,
            Fortune = being.EntropyState,
            PendingOptions = SerializePlayerOptions(playerEntity)
        };
    }

    public static PlayerOptionsData? SerializePlayerOptions(GameEntity playerEntity)
    {
        var presentation = playerEntity.Manager.PlayerChoiceSystem.GetPresentation(playerEntity);
        return presentation == null
            ? null
            : new PlayerOptionsData
            {
                Choice = presentation.Choice,
                Action = presentation.Action,
                Title = presentation.Title,
                Notice = presentation.Notice,
                Options = presentation.Options.Select(option => new PlayerOptionData
                {
                    Identifier = option.Identifier,
                    Title = option.Title,
                    Description = option.Description,
                    DisabledReason = option.DisabledReason
                }).ToList()
            };
    }

    public void RegisterOnGroups(GameManager manager)
    {
        manager.RoomActors.AddListener(this);
        _abilityBuilder.RegisterOnGroups(manager);
        _effectBuilder.RegisterOnGroups(manager);
    }

    public void UnregisterFromGroups(GameManager manager)
    {
        manager.RoomActors.RemoveListener(this);
        _abilityBuilder.UnregisterFromGroups(manager);
        _effectBuilder.UnregisterFromGroups(manager);
        _logBuilder.Unregister();
    }

    void IEntityChangeListener<GameEntity>.OnEntityAdded(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnEntityRemoved(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
        => HandleScalarPropertyChanges(entityChange);

    private bool HasScalarChanges() => _pendingScalars.DirtyMask != 0;

    private bool HandleScalarPropertyChanges(in EntityChange<GameEntity> entityChange)
    {
        if (entityChange.Entity.Id != _playerEntityId)
        {
            return false;
        }

        var changes = entityChange.PropertyChanges;
        var handled = false;
        for (var i = 0; i < changes.Count; i++)
        {
            var componentId = changes.GetChangedComponent(i).ComponentId;
            var propertyName = changes.GetChangedPropertyName(i);

            if (componentId == (int)EntityComponent.Player)
            {
                if (propertyName == nameof(PlayerComponent.NextActionTick))
                {
                    _pendingScalars.NextActionTick = entityChange.Entity.Player!.NextActionTick ?? 0;
                    _pendingScalars.DirtyMask |= NextActionTickDirty;
                    handled = true;
                }
                else if (propertyName == nameof(PlayerComponent.PendingChoice))
                {
                    _pendingScalars.DirtyMask |= PendingOptionsDirty;
                    handled = true;
                }
            }
            else if (componentId == (int)EntityComponent.Being)
            {
                switch (propertyName)
                {
                    case nameof(BeingComponent.ExperiencePoints):
                        _pendingScalars.XP = entityChange.Entity.Being!.ExperiencePoints;
                        _pendingScalars.DirtyMask |= XpDirty;
                        handled = true;
                        break;
                    case nameof(BeingComponent.HitPoints):
                        _pendingScalars.Hp = entityChange.Entity.Being!.HitPoints;
                        _pendingScalars.DirtyMask |= HpDirty;
                        handled = true;
                        break;
                    case nameof(BeingComponent.HitPointMaximum):
                        _pendingScalars.MaxHp = entityChange.Entity.Being!.HitPointMaximum;
                        _pendingScalars.DirtyMask |= MaxHpDirty;
                        handled = true;
                        break;
                    case nameof(BeingComponent.EnergyPoints):
                        _pendingScalars.Ep = entityChange.Entity.Being!.EnergyPoints;
                        _pendingScalars.DirtyMask |= EpDirty;
                        handled = true;
                        break;
                    case nameof(BeingComponent.EnergyPointMaximum):
                        _pendingScalars.MaxEp = entityChange.Entity.Being!.EnergyPointMaximum;
                        _pendingScalars.DirtyMask |= MaxEpDirty;
                        handled = true;
                        break;
                    case nameof(BeingComponent.ReservedEnergyPoints):
                        _pendingScalars.ReservedEp = entityChange.Entity.Being!.ReservedEnergyPoints;
                        _pendingScalars.DirtyMask |= ReservedEpDirty;
                        handled = true;
                        break;
                    case nameof(BeingComponent.EntropyState):
                        _pendingScalars.Fortune = entityChange.Entity.Being!.EntropyState;
                        _pendingScalars.DirtyMask |= FortuneDirty;
                        handled = true;
                        break;
                    case nameof(BeingComponent.CanPerceiveIntention):
                        handled = true;
                        break;
                }
            }
        }

        return handled;
    }
}
