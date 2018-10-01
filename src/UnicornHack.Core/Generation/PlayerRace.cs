using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Data.Players;
using UnicornHack.Primitives;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils.DataLoading;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Generation
{
    public class PlayerRace : Being, ICSScriptSerializable, ILoadable
    {
        public RaceComponent AddToAppliedEffect(GameEntity appliedEffectEntity, int beingId)
        {
            var race = AddRace(appliedEffectEntity);

            appliedEffectEntity.Ability.OwnerId = beingId;

            return race;
        }

        public static GameEntity InstantiatePlayer(string name, Sex sex, GameEntity levelEntity, Point cell)
        {
            var manager = levelEntity.Manager;
            using (var playerEntityReference = manager.CreateEntity())
            {
                var playerEntity = playerEntityReference.Referenced;

                var player = manager.CreateComponent<PlayerComponent>(EntityComponent.Player);
                player.ProperName = name;

                playerEntity.Player = player;

                var being = manager.CreateComponent<BeingComponent>(EntityComponent.Being);
                being.Sex = sex;

                playerEntity.Being = being;

                var sensor = manager.CreateComponent<SensorComponent>(EntityComponent.Sensor);

                playerEntity.Sensor = sensor;

                var physical = manager.CreateComponent<PhysicalComponent>(EntityComponent.Physical);
                physical.Material = Material.Flesh;
                physical.Capacity = ItemMovingSystem.DefaultInventorySize;

                playerEntity.Physical = physical;

                var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
                position.LevelId = levelEntity.Id;
                position.LevelCell = cell;
                position.Heading = Direction.South;

                playerEntity.Position = position;

                using (var raceEntityReference = manager.CreateEntity())
                {
                    var raceEffectEntity = raceEntityReference.Referenced;
                    var raceEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                    raceEffect.DurationTicks = (int)EffectDuration.Infinite;
                    raceEffect.EffectType = EffectType.ChangeRace;

                    raceEffectEntity.Effect = raceEffect;

                    var race = PlayerRaceData.Human.AddRace(raceEffectEntity);

                    raceEffectEntity.Ability.OwnerId = playerEntity.Id;
                    raceEffect.AffectedEntityId = playerEntity.Id;

                    manager.KnowledgeSystem.UpdateNextLevelXP(race, playerEntity);
                }

                return playerEntity;
            }
        }

        public static readonly CSScriptLoader<PlayerRace> Loader =
            new CSScriptLoader<PlayerRace>(@"Data\Players\", typeof(PlayerRaceData));

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<PlayerRace>(GetPropertyConditions<PlayerRace>());

        protected static Dictionary<string, Func<TPlayerRace, object, bool>> GetPropertyConditions<TPlayerRace>()
            where TPlayerRace : PlayerRace => new Dictionary<string, Func<TPlayerRace, object, bool>>
        {
            {nameof(Name), (o, v) => v != null},
            {nameof(Species), (o, v) => v != null},
            {nameof(SpeciesClass), (o, v) => v != null},
            {nameof(Abilities), (o, v) => ((ICollection<Ability>)v).Count != 0}
        };

        ICSScriptSerializer ICSScriptSerializable.GetSerializer() => Serializer;
    }
}
