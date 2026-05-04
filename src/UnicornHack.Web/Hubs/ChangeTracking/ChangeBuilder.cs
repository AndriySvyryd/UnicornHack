using System.Collections;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Hubs.ChangeTracking;

/// <summary>
///     Shared state machine for <c>*ChangeBuilder</c> classes. Captures Added/Modified/Removed
///     transitions in one place so subclasses only declare category-specific bits:
///     which groups/relationships to subscribe to, which entities are relevant, how to
///     translate property changes onto <typeparamref name="TChange" />, how to produce full
///     snapshots, and any deferred resolution that must happen at serialization time.
/// </summary>
/// <remarks>
///     <para>
///         Lifecycle per turn:
///         <c>RegisterOnGroups</c> (once) → listener events accumulate into <c>_changes</c> →
///         <see cref="WriteTo" /> (once) → <see cref="Clear" />.
///     </para>
///     <para>State model:</para>
///     <list type="bullet">
///         <item>
///             <description>
///                 <see cref="IChangeWithState.State" /> on each entry tracks the current
///                 pending change kind (Added/Modified/Deleted). It is set on the first
///                 event for the entity in a cycle and may transition as additional events
///                 arrive.
///             </description>
///         </item>
///         <item>
///             <description>
///                 The pre-cycle "did the client know this entity" bit is tracked
///                 out-of-band by <c>_snapshots</c>: snapshot key presence ⇔ the client knew
///                 this entity at the start of the cycle. Snapshots are populated lazily on
///                 the first event for an entity that was previously known (Property or
///                 Remove); the <see cref="HandleAdded" /> path explicitly does <i>not</i>
///                 capture a snapshot because the entity is brand-new this cycle.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Subclasses that need property-by-property reconciliation
///                 (<see cref="OnReconcileAfterReAdd" />) override
///                 <see cref="CaptureSnapshot" /> to populate the snapshot's <i>value</i>
///                 with pre-cycle values; subclasses that only need the presence bit leave
///                 the value <c>null</c> (the default).
///             </description>
///         </item>
///     </list>
///     <para>Invariants enforced by this base:</para>
///     <list type="bullet">
///         <item>
///             <description>
///                 By the time <see cref="WriteTo" /> runs, every surviving entry has
///                 <c>State</c> set — asserted in debug builds.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Add-followed-by-Remove inside one cycle cancels iff the client never saw
///                 the entity (no snapshot). Implemented by removing the entry from
///                 <c>_changes</c> in <see cref="HandleRemoved" />.
///             </description>
///         </item>
///         <item>
///             <description>
///                 <see cref="WriteTo" /> followed by another <see cref="WriteTo" /> without an
///                 intervening <see cref="Clear" /> is a caller bug — asserted in debug builds.
///             </description>
///         </item>
///     </list>
///     <para>Event sources:</para>
///     <list type="bullet">
///         <item>
///             <description>
///                 <b>Base group</b> (<see cref="IEntityChangeListener{TEntity}" />): only
///                 Property events are consumed, via <see cref="OnBaseGroupPropertyValuesChanged" />.
///                 Default routes them to <see cref="TrackPropertyChanges" /> only when an
///                 entry already exists; otherwise the event is ignored. Add/Remove on the
///                 base group are intentionally unhandled — the dependent relationship is
///                 authoritative for entity lifecycle.
///             </description>
///         </item>
///         <item>
///             <description>
///                 <b>Dependent relationship</b> (<see cref="IDependentEntityChangeListener{TEntity}" />):
///                 Add → <see cref="HandleAdded" />; Remove → <see cref="HandleRemoved" />;
///                 Property → <see cref="GetOrCreateChange" /> with <see cref="EntityState.Modified" />
///                 + <see cref="TrackPropertyChanges" />.
///             </description>
///         </item>
///     </list>
///     <para>
///         Per-entity transitions. Notation: <c>(State, hasSnapshot) + Event → (State, hasSnapshot)</c>.
///     </para>
///     <code>
///     (none, false)        + Added     → (Added, false)
///     (none, false)        + Removed   → (Deleted, true)            // captures snapshot
///     (none, false)        + Property  → (Modified, true)           // captures snapshot
///
///     (Added, false)       + Added     → (Added, false)             // no-op
///     (Added, false)       + Removed   → entry deleted               // cancel: client never saw it
///     (Added, false)       + Property  → (Added, false)             // bits accumulate, state unchanged
///
///     (Modified, true)     + Added     → (Added, true)              // re-emit as delta
///     (Modified, true)     + Removed   → (Deleted, true)
///     (Modified, true)     + Property  → (Modified, true)
///
///     (Deleted, true)      + Added     → (Modified, true)           // re-add: invokes OnReconcileAfterReAdd
///     (Deleted, true)      + Removed   → (Deleted, true)            // no-op
///     (Deleted, true)      + Property  → (Deleted, true)            // bits ignored at emission
///     </code>
///     <para>Emission at <see cref="WriteTo" /> dispatches on (State, hasSnapshot):</para>
///     <list type="bullet">
///         <item><description><c>(null, *)</c> — skipped (entry never resolved).</description></item>
///         <item><description><c>(Added, false)</c> — full snapshot via <see cref="ProduceFullSnapshot" />.</description></item>
///         <item><description><c>(Added, true)</c> — delta via <see cref="ProduceDelta" /> (re-emission after Modified→Added).</description></item>
///         <item><description><c>(Modified, false)</c> — full snapshot (impossible under the table above; defensive).</description></item>
///         <item><description><c>(Modified, true)</c> — delta via <see cref="ProduceDelta" />.</description></item>
///         <item><description><c>(Deleted, false)</c> — impossible (HandleRemoved either captures a snapshot or cancels).</description></item>
///         <item><description><c>(Deleted, true)</c> — removal sentinel.</description></item>
///     </list>
/// </remarks>
public abstract class ChangeBuilder<TChange>
    : IEntityChangeListener<GameEntity>, IDependentEntityChangeListener<GameEntity>
    where TChange : class, IChangeWithState, new()
{
    private readonly Dictionary<int, TChange> _changes = [];
    private readonly Dictionary<int, TChange?> _snapshots = [];
    private bool _wroteWithoutClear;

    /// <summary>True if any changes have been recorded since the last <see cref="Clear" />.</summary>
    public bool HasChanges => _changes.Count > 0;

    /// <summary>
    ///     The pending change entries, keyed by entity id. Exposed for subclasses that need
    ///     to route events across entities (e.g. a sibling-component change routed to a
    ///     knowledge entity).
    /// </summary>
    protected Dictionary<int, TChange> Changes => _changes;

    /// <summary>
    ///     Per-cycle snapshot dictionary. Key presence ⇔ "the client knew this entity at
    ///     the start of the current cycle". Snapshot values are populated by
    ///     <see cref="CaptureSnapshot" /> for builders that need property-by-property
    ///     reconciliation; otherwise the value is <c>null</c> and only key presence is used.
    /// </summary>
    protected Dictionary<int, TChange?> Snapshots => _snapshots;

    #region Subscription hooks

    public abstract void RegisterOnGroups(GameManager manager);

    public abstract void UnregisterFromGroups(GameManager manager);

    #endregion

    #region Filtering hooks

    /// <summary>
    ///     True if the entity in its current state is tracked by this builder. Called on
    ///     relationship Add/Remove events. Default: always true.
    /// </summary>
    protected virtual bool IsRelevant(GameEntity entity) => true;

    /// <summary>
    ///     True if the relationship principal is one this builder cares about. Used by
    ///     per-player builders to filter by owner. Default: always true.
    /// </summary>
    protected virtual bool IsRelevantPrincipal(GameEntity principal) => true;

    /// <summary>
    ///     True if the entity, at the time of a Remove event, was previously relevant —
    ///     used when relevance depends on a component that has just been removed.
    ///     Default: false.
    /// </summary>
    protected virtual bool IsRelevantOnRemove(in EntityChange<GameEntity> entityChange) => false;

    #endregion

    #region Property-tracking hooks

    /// <summary>
    ///     Apply per-property changes from <paramref name="changes" /> onto
    ///     <paramref name="change" />. Typical implementation switches on the component id
    ///     and property name. Default: no-op (for builders that re-serialize from scratch).
    /// </summary>
    protected virtual void TrackPropertyChanges(
        GameEntity entity, TChange change, IPropertyValueChanges changes)
    {
    }

    /// <summary>
    ///     Handles a property-change event delivered via <see cref="IEntityChangeListener{TEntity}" />
    ///     on a base group (not a relationship). Default:
    ///     <list type="bullet">
    ///         <item><description>If the entity is already tracked, forward to <see cref="TrackPropertyChanges" />.</description></item>
    ///         <item><description>Otherwise, ignore (entity is not relevant yet — relationship events haven't created the entry).</description></item>
    ///     </list>
    ///     Override when the base group fires events for sibling entities that must be routed
    ///     to a different tracked entity, or when relevance can transition via a property
    ///     change (e.g. a slot going from <c>null</c> to non-<c>null</c>).
    /// </summary>
    protected virtual void OnBaseGroupPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        var entity = entityChange.Entity;
        if (_changes.TryGetValue(entity.Id, out var tracked))
        {
            TrackPropertyChanges(entity, tracked, entityChange.PropertyChanges);
        }
    }

    #endregion

    #region Serialization hooks

    /// <summary>
    ///     Produces a fresh full snapshot of the entity. Used for Added entries (client
    ///     doesn't know the entity yet) and for the re-Added path.
    /// </summary>
    protected abstract TChange ProduceFullSnapshot(GameEntity entity, SerializationContext context);

    /// <summary>
    ///     Produces a delta for a Modified entry where the client already knew the entity.
    ///     Default: returns the accumulated <paramref name="tracked" /> change iff any bits
    ///     beyond the reserved index 0 are set; otherwise returns <c>null</c> (no-op emission).
    ///     Override for re-serialize-from-scratch builders or when additional validation is needed.
    /// </summary>
    protected virtual TChange? ProduceDelta(GameEntity entity, TChange tracked, SerializationContext context)
        => HasModifiedBits(tracked.ChangedProperties) ? tracked : null;

    /// <summary>
    ///     Hook invoked when an entry transitions <see cref="EntityState.Deleted" /> →
    ///     <see cref="EntityState.Modified" /> within a single cycle (the entity left the
    ///     client's view and re-entered before <see cref="WriteTo" />). Subclasses that
    ///     accumulate per-property bits in <paramref name="tracked" /> may override to clear
    ///     bits that round-tripped back to the client's last-known value. The pre-cycle
    ///     baseline is available via <see cref="Snapshots" />. Default: no-op.
    /// </summary>
    protected virtual void OnReconcileAfterReAdd(GameEntity entity, TChange tracked)
    {
    }

    /// <summary>
    ///     Captures a snapshot of the entity's pre-cycle state. Called lazily on the first
    ///     event for an entity that was previously known to the client (Property or Remove);
    ///     <i>not</i> called for HandleAdded as the first event, because the entity is
    ///     brand-new this cycle.
    ///     <para>
    ///         <paramref name="changes" /> carries this event's property changes when the
    ///         triggering event is a Property change; subclasses may use <c>Old</c> values
    ///         from there to override fields that are about to mutate. When the triggering
    ///         event is a Remove, <paramref name="changes" /> is <c>null</c> and the current
    ///         entity state is the pre-cycle state.
    ///     </para>
    ///     <para>
    ///         Default: returns <c>null</c>. Subclasses that need property-by-property
    ///         reconciliation override to return a populated <typeparamref name="TChange" />.
    ///     </para>
    /// </summary>
    protected virtual TChange? CaptureSnapshot(GameEntity entity, IPropertyValueChanges? changes) => null;

    /// <summary>
    ///     Hook invoked from <see cref="WriteTo" /> after each entry has been dispatched
    ///     to <paramref name="output" /> (or skipped). Subclasses use this to maintain
    ///     persistent per-emission state across cycles — for example, caching the last
    ///     value sent to the client for a property that should only re-emit when changed.
    ///     <para>
    ///         <paramref name="emitted" /> is the value placed in <paramref name="output" />
    ///         for this id, or <c>null</c> if nothing was emitted (cancelled or no-op delta).
    ///     </para>
    ///     Default: no-op.
    /// </summary>
    protected virtual void OnAfterEmit(int id, GameEntity? entity, TChange? emitted, EntityState dispatchedState)
    {
    }

    #endregion

    #region State-machine primitives (protected so subclasses with custom routing can reuse)

    /// <summary>
    ///     Ensures a snapshot entry exists for <paramref name="entity" />. Called by the
    ///     state-machine primitives when the first event for an entity is Property or
    ///     Remove (i.e. the client knew the entity before this cycle).
    /// </summary>
    protected void EnsureSnapshot(GameEntity entity, IPropertyValueChanges? changes = null)
    {
        if (_snapshots.ContainsKey(entity.Id))
        {
            return;
        }

        _snapshots[entity.Id] = CaptureSnapshot(entity, changes);
    }

    protected void HandleAdded(GameEntity entity)
    {
        if (_changes.TryGetValue(entity.Id, out var tracked))
        {
            tracked.Entity = entity;
            switch (tracked.State)
            {
                case EntityState.Deleted:
                    Debug.Assert(_snapshots.ContainsKey(entity.Id),
                        "Deleted state implies HandleRemoved/Property ran, which captures a snapshot.");
                    OnReconcileAfterReAdd(entity, tracked);
                    tracked.State = EntityState.Modified;
                    break;
                case EntityState.Modified:
                case EntityState.Added:
                case null:
                    tracked.State = EntityState.Added;
                    break;
            }
        }
        else
        {
            _changes[entity.Id] = new TChange
            {
                Entity = entity,
                State = EntityState.Added
            };
            // Brand-new entity: no snapshot captured.
        }
    }

    protected void HandleRemoved(GameEntity entity)
    {
        if (_changes.TryGetValue(entity.Id, out var tracked))
        {
            tracked.Entity = entity;
            if (tracked.State == EntityState.Added && !_snapshots.ContainsKey(entity.Id))
            {
                // Add-then-Remove on a brand-new entity: cancel.
                _changes.Remove(entity.Id);
            }
            else
            {
                tracked.State = EntityState.Deleted;
            }
        }
        else
        {
            // First event for this entity: client knew it. Capture pre-cycle snapshot.
            EnsureSnapshot(entity);
            _changes[entity.Id] = new TChange
            {
                Entity = entity,
                State = EntityState.Deleted
            };
        }
    }

    /// <summary>
    ///     Ensures an entry exists for an entity whose properties are changing. When
    ///     <paramref name="state" /> is non-null and a new entry is created, the entry's
    ///     <c>State</c> is set to <paramref name="state" /> (typically
    ///     <see cref="EntityState.Modified" /> — "client already knew this entity") and a
    ///     pre-cycle snapshot is captured from <paramref name="changes" />. For an existing
    ///     entry, neither <c>State</c> nor the snapshot is overwritten — a Property event
    ///     must not clobber state established by a relationship event (Add/Remove).
    /// </summary>
    protected TChange GetOrCreateChange(
        GameEntity entity,
        EntityState? state = null,
        IPropertyValueChanges? changes = null)
    {
        if (!_changes.TryGetValue(entity.Id, out var tracked))
        {
            tracked = new TChange { State = state, Entity = entity };
            _changes[entity.Id] = tracked;
            if (state == EntityState.Modified)
            {
                EnsureSnapshot(entity, changes);
            }
        }
        else
        {
            tracked.Entity = entity;
            tracked.State ??= state;
        }

        return tracked;
    }

    #endregion

    #region IDependentEntityChangeListener

    void IDependentEntityChangeListener<GameEntity>.OnEntityAdded(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (!IsRelevantPrincipal(principal))
        {
            return;
        }

        var entity = entityChange.Entity;
        if (!IsRelevant(entity))
        {
            return;
        }

        HandleAdded(entity);
    }

    void IDependentEntityChangeListener<GameEntity>.OnEntityRemoved(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (!IsRelevantPrincipal(principal))
        {
            return;
        }

        var entity = entityChange.Entity;
        if (!IsRelevant(entity) && !IsRelevantOnRemove(entityChange))
        {
            return;
        }

        HandleRemoved(entity);
    }

    void IDependentEntityChangeListener<GameEntity>.OnPropertyValuesChanged(
        in EntityChange<GameEntity> entityChange, GameEntity principal)
    {
        if (!IsRelevantPrincipal(principal))
        {
            return;
        }

        // The dependent listener fires for every entity in the relationship's dependent
        // group, which can be broader than this builder's interest
        // Filter out events for entities that this builder neither already tracks nor
        // would consider relevant. Already-tracked entities pass through even when
        // currently irrelevant (e.g. AbilityChangeBuilder needs property bits while an
        // ability is in its in-cycle Deleted phase so OnReconcileAfterReAdd has them
        // available if the slot is restored).
        var entity = entityChange.Entity;
        if (!_changes.ContainsKey(entity.Id) && !IsRelevant(entity))
        {
            return;
        }

        OnDependentPropertyValuesChanged(entityChange);
    }

    /// <summary>
    ///     Handles a property-change event delivered via
    ///     <see cref="IDependentEntityChangeListener{TEntity}" /> on a relationship.
    ///     Default: creates/updates a Modified entry and forwards to
    ///     <see cref="TrackPropertyChanges" />.
    ///     <para>
    ///         Override to suppress dependent property events when the same property
    ///         changes are already being handled via the base group (e.g. when a builder
    ///         subscribes to both the base group and a relationship that re-broadcasts the
    ///         same property events — having both fire would cause the dependent path to
    ///         re-create entries the base group has just cancelled).
    ///     </para>
    /// </summary>
    protected virtual void OnDependentPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        var tracked = GetOrCreateChange(entityChange.Entity, EntityState.Modified, entityChange.PropertyChanges);
        TrackPropertyChanges(entityChange.Entity, tracked, entityChange.PropertyChanges);
    }

    #endregion

    #region IEntityChangeListener

    void IEntityChangeListener<GameEntity>.OnEntityAdded(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnEntityRemoved(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
        => OnBaseGroupPropertyValuesChanged(entityChange);

    #endregion

    #region Emission

    /// <summary>
    ///     Writes all pending changes into <paramref name="output" />. Contract: call at
    ///     most once per <see cref="Clear" /> cycle. Calling twice without an intervening
    ///     <see cref="Clear" /> would cause the same entries to be emitted into two
    ///     different change sets, corrupting the client's view — asserted in debug builds.
    /// </summary>
    public void WriteTo(Dictionary<int, TChange> output, SerializationContext context)
    {
        Debug.Assert(!_wroteWithoutClear,
            $"{GetType().Name}.WriteTo called twice without Clear. "
            + "This indicates a duplicate listener registration or a missing Clear.");
        _wroteWithoutClear = true;

        foreach (var (id, change) in _changes)
        {
            if (change.State == null)
            {
                continue;
            }

            var hasSnapshot = _snapshots.ContainsKey(id);
            var dispatchedState = change.State.Value;
            TChange? emitted = null;

            switch (change.State)
            {
                case EntityState.Added:
                {
                    var entity = change.Entity;
                    if (entity == null)
                    {
                        break;
                    }

                    if (!hasSnapshot)
                    {
                        emitted = ProduceFullSnapshot(entity, context);
                        output[id] = emitted;
                    }
                    else
                    {
                        // Entity was previously known and re-emitted as Added (Modified→Added
                        // transition). Send a delta — the client already knows it.
                        emitted = ProduceDelta(entity, change, context);
                        if (emitted != null)
                        {
                            output[id] = emitted;
                        }
                    }

                    break;
                }

                case EntityState.Modified:
                {
                    var entity = change.Entity;
                    if (entity == null)
                    {
                        break;
                    }

                    if (!hasSnapshot)
                    {
                        // Defensive: under the documented transition table this combination
                        // is unreachable. Treat as full snapshot to be safe.
                        emitted = ProduceFullSnapshot(entity, context);
                        output[id] = emitted;
                        break;
                    }

                    emitted = ProduceDelta(entity, change, context);
                    if (emitted != null)
                    {
                        output[id] = emitted;
                    }

                    break;
                }

                case EntityState.Deleted:
                    // Only send a removal if the client actually knew the entity.
                    if (hasSnapshot)
                    {
                        emitted = new TChange { ChangedProperties = new BitArray(1) };
                        output[id] = emitted;
                    }

                    break;
            }

            OnAfterEmit(id, change.Entity, emitted, dispatchedState);
        }
    }

    public void Clear()
    {
        _changes.Clear();
        _snapshots.Clear();
        _wroteWithoutClear = false;
    }

    #endregion

    protected static bool HasModifiedBits(BitArray? bits)
    {
        if (bits == null)
        {
            return false;
        }

        var hadFirstBit = bits[0];
        bits[0] = false;
        var hasAny = bits.HasAnySet();
        bits[0] = hadFirstBit;
        return hasAny;
    }
}
