using UnicornHack.Systems.Beings;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Knowledge;

/// <summary>
///     Handles player XP
/// </summary>
public class XPSystem :
    IGameSystem<KnownTerrainChangedMessage>,
    IGameSystem<DiedMessage>
{
    public MessageProcessingResult Process(KnownTerrainChangedMessage message, GameManager manager)
    {
        if (message.TilesExplored > 0)
        {
            AddPlayerXP(message.TilesExplored * message.RoomEntity.Room!.Level.Difficulty, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(DiedMessage message, GameManager manager)
    {
        if (!message.BeingEntity.HasComponent(EntityComponent.Player))
        {
            var xp = message.BeingEntity.Being!.ExperiencePoints;
            if (xp == 0)
            {
                xp = 100 + message.BeingEntity.Being.Races.Sum(race => race.Ability!.Level) * 10;
            }

            if (xp > 0)
            {
                AddPlayerXP(xp, manager);
            }
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public void AddPlayerXP(int xp, GameManager manager)
    {
        foreach (var playerEntity in manager.Players)
        {
            AddXP(playerEntity, xp, manager);
            manager.EffectApplicationSystem.RealizeXpExpiringEffects(playerEntity, xp, manager);
        }

        if (!manager.XPCooldownEntities.Any())
        {
            return;
        }

        foreach (var xpEntity in manager.XPCooldownEntities.ToList())
        {
            var ability = xpEntity.Ability;
            if (ability?.CooldownXpLeft != null)
            {
                // TODO: Perf: Queue as a change value message and remove .ToList()
                var newCooldown = ability.CooldownXpLeft - xp;
                ability.CooldownXpLeft = newCooldown > 0 ? newCooldown : null;
            }
        }
    }

    private void AddXP(GameEntity actorEntity, int xp, GameManager manager)
    {
        var being = actorEntity.Being!;
        being.ExperiencePoints += xp;

        var xpGained = XPGainedMessage.Create(manager);
        xpGained.Entity = actorEntity;
        xpGained.ExperiencePoints = xp;
        manager.Enqueue(xpGained);
    }

}
