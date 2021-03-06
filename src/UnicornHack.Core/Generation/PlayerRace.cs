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
        public int SkillPointRate { get; set; }
        public int TraitPointRate { get; set; }
        public int MutationPointRate { get; set; }

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
                player.NextActionTick = manager.Game.CurrentTick;
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
                    raceEffect.Duration = EffectDuration.Infinite;
                    raceEffect.EffectType = EffectType.ChangeRace;

                    raceEffectEntity.Effect = raceEffect;

                    PlayerRaceData.Human.AddRace(raceEffectEntity);

                    raceEffectEntity.Ability.OwnerEntity = playerEntity;
                    raceEffect.AffectedEntityId = playerEntity.Id;
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
            {nameof(Name), (_, v) => v != null},
            {nameof(Species), (_, v) => v != null},
            {nameof(SpeciesClass), (_, v) => v != null},
            {nameof(Abilities), (_, v) => ((ICollection<Ability>)v).Count != 0},
            {nameof(SkillPointRate), (_, v) => (int)v != 0},
            {nameof(TraitPointRate), (_, v) => (int)v != 0},
            {nameof(MutationPointRate), (_, v) => (int)v != 0}
        };

        ICSScriptSerializer ICSScriptSerializable.GetSerializer() => Serializer;
    }
}
