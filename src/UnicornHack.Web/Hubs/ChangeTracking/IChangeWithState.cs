using System.Collections;

namespace UnicornHack.Hubs.ChangeTracking;

/// <summary>
///     Implemented by every <c>*Change</c> DTO that participates in the change-tracking
///     state machine. The properties together drive Added/Modified/Removed emission:
///     <list type="bullet">
///         <item>
///             <description>
///                 <see cref="ChangedProperties" /> (wire-level): per-property dirty bits.
///                 <c>null</c> = full snapshot; length-1 BitArray = removal sentinel.
///             </description>
///         </item>
///         <item>
///             <description>
///                 <see cref="State" />: the current pending change kind.
///                 Mutates as events arrive in the current turn.
///                 <c>null</c> = state not yet resolved.
///             </description>
///         </item>
///     </list>
///     The pre-cycle state of the entity is tracked by the builder via a per-cycle snapshot dictionary.
/// </summary>
public interface IChangeWithState
{
    GameEntity Entity
    {
        get; set;
    }

    BitArray? ChangedProperties
    {
        get; set;
    }

    EntityState? State
    {
        get; set;
    }
}
