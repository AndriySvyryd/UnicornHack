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

        AdvanceTurnMessage.Enqueue(manager);
        manager.Queue.ProcessQueue(manager);

        if (manager.Game.ActingPlayer != null)
        {
            return TurnResult.PlayerTurn;
        }

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
        // modifier expiry. Realize a modifier expiry that is strictly earlier than the action first, so its
        // contribution is gone before the actor reads any resolved value (a tie goes to the action).
        var actionTick = GetTick(entity);
        var effectTick = entity.ActiveEffects?.NextEffectTick;
        if (effectTick != null
            && (actionTick == null || effectTick.Value < actionTick.Value))
        {
            manager.Game.CurrentTick = effectTick.Value;
            manager.EffectApplicationSystem.ExpireTimedPropertyEffects(entity, manager);
            return MessageProcessingResult.ContinueProcessing;
        }

        Debug.Assert(actionTick != null, $"Entity {entity.Id} is not a supported temporal entity!");
        manager.Game.CurrentTick = actionTick!.Value;

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
            && ability.CooldownTick == actionTick)
        {
            ability.CooldownTick = null;
            return MessageProcessingResult.ContinueProcessing;
        }

        return MessageProcessingResult.StopProcessing;
    }

    public static int? GetTick(GameEntity entity)
    {
        var ai = entity.AI;
        if (ai != null)
        {
            return ai.NextActionTick;
        }

        var player = entity.Player;
        if (player != null)
        {
            return player.NextActionTick;
        }

        var ability = entity.Ability;
        if (ability != null)
        {
            return ability.CooldownTick;
        }

        return null;
    }
}
