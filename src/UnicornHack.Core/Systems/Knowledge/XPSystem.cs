using System;
using System.Linq;
using UnicornHack.Generation;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Knowledge
{
    /// <summary>
    ///     Handles player XP
    /// </summary>
    public class XPSystem :
        IGameSystem<KnownTerrainChangedMessage>,
        IGameSystem<DiedMessage>,
        IGameSystem<EntityAddedMessage<GameEntity>>,
        IGameSystem<EntityRemovedMessage<GameEntity>>
    {
        public MessageProcessingResult Process(KnownTerrainChangedMessage message, GameManager manager)
        {
            if (message.TilesExplored > 0)
            {
                AddPlayerXP(message.TilesExplored * message.LevelEntity.Level.Difficulty, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(DiedMessage message, GameManager manager)
        {
            if (!message.BeingEntity.HasComponent(EntityComponent.Player))
            {
                var xp = message.BeingEntity.Being.ExperiencePoints;
                if (xp == 0)
                {
                    var level = GetXPLevel(message.BeingEntity);
                    xp = 100 + level * 10;
                }
                AddPlayerXP(xp, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
        {
            var affectedEntity = manager.FindEntity(message.Entity.Effect?.AffectedEntityId);
            if (affectedEntity?.HasComponent(EntityComponent.Player) != true)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            manager.XPSystem.UpdateNextLevelXP(affectedEntity);
            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(EntityRemovedMessage<GameEntity> message, GameManager manager)
        {
            var effect = message.Entity.Effect ?? message.RemovedComponent as EffectComponent;
            var player = manager.FindEntity(effect?.AffectedEntityId)?.Player;
            if (player?.Entity.Being.IsAlive != true)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            var race = message.Entity.Race ?? message.RemovedComponent as RaceComponent;
            var template = PlayerRace.Loader.Find(race.TemplateName);
            var levelsLost = race.Level - 1;
            player.SkillPoints -= levelsLost * template.SkillPointRate;
            player.TraitPoints -= levelsLost * template.TraitPointRate;
            player.MutationPoints -= levelsLost * template.MutationPointRate;

            manager.XPSystem.UpdateNextLevelXP(player.Entity);
            return MessageProcessingResult.ContinueProcessing;
        }

        public void AddPlayerXP(int xp, GameManager manager)
        {
            foreach (var playerEntity in manager.Players)
            {
                AddXP(playerEntity, xp, manager);
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

                var effect = xpEntity.Effect;
                if (effect?.ExpirationXp != null)
                {
                    var newExpiration = effect.ExpirationXp - xp;
                    if (newExpiration < 0)
                    {
                        RemoveComponentMessage.Enqueue(xpEntity, EntityComponent.Effect, manager);
                    }
                    else
                    {
                        effect.ExpirationXp = newExpiration;
                    }
                }
            }
        }

        public RaceComponent GetLearningRace(GameEntity actorEntity)
            => actorEntity.Being.Races.Select(r => r.Race).OrderBy(r => r.Level).First();

        public byte GetXPLevel(GameEntity actorEntity)
            => (byte)actorEntity.Being.Races.Sum(r => r.Race.Level);

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

                    var race = GetLearningRace(actorEntity);
                    race.Level++;

                    var template = PlayerRace.Loader.Find(race.TemplateName);
                    player.SkillPoints += template.SkillPointRate;
                    player.TraitPoints += template.TraitPointRate;
                    player.MutationPoints += template.MutationPointRate;

                    UpdateNextLevelXP(actorEntity);

                    var leveledUp = LeveledUpMessage.Create(manager);
                    leveledUp.Entity = actorEntity;
                    leveledUp.Race = race;
                    leveledUp.SkillPointsGained = template.SkillPointRate;
                    leveledUp.TraitPointsGained = template.TraitPointRate;
                    leveledUp.MutationPointsGained = template.MutationPointRate;
                    manager.Enqueue(leveledUp);
                }
                else
                {
                    leftoverXP = 0;
                }
            }

            var xpGained = XPGainedMessage.Create(manager);
            xpGained.Entity = actorEntity;
            xpGained.ExperiencePoints = xp;
            manager.Enqueue(xpGained);
        }

        public void UpdateNextLevelXP(GameEntity actorEntity)
        {
            var player = actorEntity.Player;
            var playerLevel = GetXPLevel(actorEntity);
            if (playerLevel > player.MaxLevel)
            {
                player.MaxLevel = playerLevel;
            }

            var effectiveLevel = player.MaxLevel > playerLevel ? player.MaxLevel - playerLevel : playerLevel;
            player.NextLevelXP = (int)((2 + Math.Round(Math.Pow(effectiveLevel, 1.5) * 2)) * 250);
        }
    }
}
