using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Data.Players;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
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

        public static GameEntity InstantiatePlayer(string name, Sex sex, LevelComponent level, Point cell)
        {
            var manager = level.Entity.Manager;
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
                playerEntity.Physical = physical;

                var position = manager.CreateComponent<PositionComponent>(EntityComponent.Position);
                position.LevelId = level.EntityId;
                position.LevelCell = cell;
                position.Heading = Direction.South;

                playerEntity.Position = position;

                var defaultRace = PlayerRaceData.Human;

                using (var innateAbilityReference = manager.CreateEntity())
                {
                    var innateAbilityEntity = innateAbilityReference.Referenced;
                    var innateEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                    innateEffect.EffectType = EffectType.AddAbility;
                    innateEffect.Duration = EffectDuration.Infinite;

                    innateAbilityEntity.Effect = innateEffect;

                    var innateAbility = manager.CreateComponent<AbilityComponent>(EntityComponent.Ability);
                    innateAbility.Name = EffectApplicationSystem.InnateAbilityName;
                    innateAbility.Activation = ActivationType.Always;
                    innateAbility.SuccessCondition = AbilitySuccessCondition.Always;

                    innateAbilityEntity.Ability = innateAbility;

                    defaultRace.AddPropertyEffect(nameof(PhysicalComponent.Material), (int?)Material.Flesh,
                        innateAbilityEntity.Id, manager, ValueCombinationFunction.Override);
                    defaultRace.AddPropertyEffect(nameof(PhysicalComponent.Capacity),
                        AbilitySlottingSystem.DefaultSlotCapacity + 2, innateAbilityEntity.Id, manager);

                    innateAbility.OwnerEntity = playerEntity;
                    innateEffect.AffectedEntity = playerEntity;
                }

                using (var raceEntityReference = manager.CreateEntity())
                {
                    var raceEffectEntity = raceEntityReference.Referenced;
                    var raceEffect = manager.CreateComponent<EffectComponent>(EntityComponent.Effect);
                    raceEffect.EffectType = EffectType.ChangeRace;
                    raceEffect.Duration = EffectDuration.Infinite;

                    raceEffectEntity.Effect = raceEffect;

                    defaultRace.AddToAppliedEffect(raceEffectEntity, playerEntity);

                    raceEffect.AffectedEntity = playerEntity;
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
