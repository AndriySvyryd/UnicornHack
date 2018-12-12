using System;
using System.Linq;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Knowledge
{
    public class XPSystem:
        IGameSystem<VisibleTerrainChangedMessage>,
        IGameSystem<DiedMessage>
    {
        public const string XPGainedMessageName = "XPGained";

        public MessageProcessingResult Process(VisibleTerrainChangedMessage message, GameManager manager)
        {
            if (message.TilesExplored > 0)
            {
                AddPlayerXP(message.TilesExplored, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(DiedMessage message, GameManager manager)
        {
            if (!message.BeingEntity.HasComponent(EntityComponent.Player))
            {
                AddPlayerXP(GetXPLevel(message.BeingEntity, manager) * 100, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public void AddPlayerXP(int xp, GameManager manager)
        {
            foreach (var playerEntity in manager.Players)
            {
                AddXP(playerEntity, xp, manager);
            }
        }

        public RaceComponent GetLearningRace(GameEntity actorEntity, GameManager manager)
            => manager.RacesToBeingRelationship[actorEntity.Id].Values.First().Race;

        public byte GetXPLevel(GameEntity actorEntity, GameManager manager)
            => (byte)manager.RacesToBeingRelationship[actorEntity.Id].Values.Sum(r => r.Race.Level);

        private void AddXP(GameEntity actorEntity, int xp, GameManager manager)
        {
            var player = actorEntity.Player;
            var being = actorEntity.Being;
            var leftoverXP = xp;
            while (leftoverXP != 0)
            {
                being.ExperiencePoints += leftoverXP;
                if (being.ExperiencePoints >= player.NextLevelXP)
                {
                    leftoverXP = being.ExperiencePoints - player.NextLevelXP;
                    being.ExperiencePoints = 0;

                    // TODO: fire an event and trigger abilities
                    var race = GetLearningRace(actorEntity, manager);
                    race.Level++;

                    UpdateNextLevelXP(actorEntity);
                }
                else
                {
                    leftoverXP = 0;
                }
            }

            var xpGained = manager.Queue.CreateMessage<XPGainedMessage>(XPGainedMessageName);
            xpGained.Entity = actorEntity;
            xpGained.ExperiencePoints = xp;
            manager.Enqueue(xpGained);
        }

        public void UpdateNextLevelXP(GameEntity actorEntity)
        {
            var player = actorEntity.Player;
            var playerLevel = GetXPLevel(actorEntity, actorEntity.Manager);
            if (playerLevel > player.MaxLevel)
            {
                player.MaxLevel = playerLevel;
            }

            var effectiveLevel = player.MaxLevel > playerLevel ? player.MaxLevel - playerLevel : playerLevel;
            player.NextLevelXP = (int)((1 + Math.Ceiling(Math.Pow(effectiveLevel, 1.5))) * 500);
        }
    }
}
