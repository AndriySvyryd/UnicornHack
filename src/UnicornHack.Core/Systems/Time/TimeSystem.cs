using UnicornHack.Systems.Actors;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Time;

public class TimeSystem : IGameSystem<AdvanceTurnMessage>
{
    public const int DefaultActionDelay = 100;

    public TurnResult AdvanceSingleTurn(GameManager manager)
    {
        if (!manager.Players.Any(p => p.Being!.IsAlive)
            || manager.TemporalEntitiesIndex.Count == 0)
        {
            return TurnResult.GameOver;
        }

        if (IsWaitingForPlayerInput(manager))
        {
            return TurnResult.PlayerTurn;
        }

#if DEBUG
        var entity = manager.TemporalEntitiesIndex.First();
        var actionTick = entity.AI?.NextActionTick
                         ?? entity.Player?.NextActionTick
                         ?? entity.Ability?.CooldownTick;
        var processingAction = actionTick is { } scheduledActionTick
                               && (entity.ActiveEffects?.NextEffectTick is not { } effectTick
                                   || scheduledActionTick < effectTick);
#endif

        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        if (IsWaitingForPlayerInput(manager))
        {
            return TurnResult.PlayerTurn;
        }

#if DEBUG
    if (processingAction
            && manager.TemporalEntitiesIndex.Count != 0
            && ReferenceEquals(manager.TemporalEntitiesIndex.First(), entity)
            && (entity.AI?.NextActionTick
                ?? entity.Player?.NextActionTick
                ?? entity.Ability?.CooldownTick) == actionTick)
        {
            var actor = (ActorComponent?)entity.AI ?? entity.Player;
            throw new InvalidOperationException(
                $"Entity {entity.Id} did not advance from tick {actionTick}. "
                + $"Action: {actor?.NextAction}; target: {actor?.NextActionTarget}; "
                + $"target2: {actor?.NextActionTarget2}.");
        }
#endif

        return TurnResult.Continue;
    }

    public MessageProcessingResult Process(AdvanceTurnMessage message, GameManager manager)
    {
        if (manager.TemporalEntitiesIndex.Count == 0)
        {
            if (manager.Queue.QueuedCount == 0)
            {
                throw new InvalidOperationException("No actors found!");
            }

            return MessageProcessingResult.StopProcessing;
        }

        var entity = manager.TemporalEntitiesIndex.First();

        // The entity is keyed at the earliest of its next action / ability cooldown and its next timed-
        // modifier expiry. Realize a modifier expiry before an action at the same tick, so its contribution
        // is gone before the actor reads any resolved value.
        var tick = GetTick(entity);
        
        Debug.Assert(tick != null, $"Entity {entity.Id} is not a supported temporal entity!");
        manager.Game.CurrentTick = tick.Value;

        if (tick == entity.ActiveEffects?.NextEffectTick)
        {
            manager.EffectApplicationSystem.ExpireTimedPropertyEffects(entity, manager);
            return MessageProcessingResult.ContinueProcessing;
        }
        
        var ai = entity.AI;
        if (ai != null)
        {
            PerformActionMessage.EnqueueAI(entity, manager);
            return MessageProcessingResult.ContinueProcessing;
        }

        var player = entity.Player;
        if (player != null)
        {
            PerformActionMessage.EnqueuePlayer(entity, manager);
            return MessageProcessingResult.ContinueProcessing;
        }
        
        var ability = entity.Ability;
        if (ability != null
            && ability.CooldownTick == tick)
        {
            ability.CooldownTick = null;
            return MessageProcessingResult.ContinueProcessing;
        }

        return MessageProcessingResult.StopProcessing;
    }

    public static int? GetTick(GameEntity entity)
    {
        var actionTick = entity.AI?.NextActionTick
                         ?? entity.Player?.NextActionTick
                         ?? entity.Ability?.CooldownTick;
        var effectTick = entity.ActiveEffects?.NextEffectTick;
        return actionTick == null || effectTick < actionTick
            ? effectTick
            : actionTick;
    }

    private static bool IsWaitingForPlayerInput(GameManager manager)
    {
        if (manager.TemporalEntitiesIndex.Count == 0)
        {
            return false;
        }

        var entity = manager.TemporalEntitiesIndex.First();
        return entity.Player is
                   { NextAction: null, NextActionTick: { } actionTick, PreviousActionTick: { } previousActionTick }
               && actionTick == manager.Game.CurrentTick
               && previousActionTick == manager.Game.CurrentTick;
    }
}
