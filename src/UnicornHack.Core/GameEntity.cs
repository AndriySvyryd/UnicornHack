using System.Collections.Generic;
using System.Diagnostics;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Effects;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;
using UnicornHack.Systems.Senses;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack
{
    public class GameEntity : Entity
    {
        private int _gameId;

        public int GameId
        {
            get => _gameId;
            // ReSharper disable once UnusedMember.Local
            private set => SetWithNotify(value, ref _gameId);
        }

        public Game Game => Manager.Game;
        public new GameManager Manager => (GameManager)base.Manager;

        public AIComponent AI
        {
            get => (AIComponent)FindComponent((int)EntityComponent.AI);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.AI);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        public PlayerComponent Player
        {
            get => (PlayerComponent)FindComponent((int)EntityComponent.Player);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.Player);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        public EffectComponent Effect
        {
            get => (EffectComponent)FindComponent((int)EntityComponent.Effect);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.Effect);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        public AbilityComponent Ability
        {
            get => (AbilityComponent)FindComponent((int)EntityComponent.Ability);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.Ability);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        public BeingComponent Being
        {
            get => (BeingComponent)FindComponent((int)EntityComponent.Being);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.Being);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        public PhysicalComponent Physical
        {
            get => (PhysicalComponent)FindComponent((int)EntityComponent.Physical);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.Physical);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        public RaceComponent Race
        {
            get => (RaceComponent)FindComponent((int)EntityComponent.Race);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.Race);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        public ItemComponent Item
        {
            get => (ItemComponent)FindComponent((int)EntityComponent.Item);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.Item);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        public LevelComponent Level
        {
            get => (LevelComponent)FindComponent((int)EntityComponent.Level);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.Level);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        public ConnectionComponent Connection
        {
            get => (ConnectionComponent)FindComponent((int)EntityComponent.Connection);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.Connection);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        public PositionComponent Position
        {
            get => (PositionComponent)FindComponent((int)EntityComponent.Position);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.Position);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        public SensorComponent Sensor
        {
            get => (SensorComponent)FindComponent((int)EntityComponent.Sensor);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.Sensor);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        public KnowledgeComponent Knowledge
        {
            get => (KnowledgeComponent)FindComponent((int)EntityComponent.Knowledge);
            set
            {
                if (value == null)
                {
                    RemoveComponent(EntityComponent.Knowledge);
                }
                else
                {
                    AddComponent(value);
                }
            }
        }

        // Used for debugging
#pragma warning disable RCS1213 // Remove unused member declaration.
        private RelationshipsView Relationships => new RelationshipsView(this);
#pragma warning restore RCS1213 // Remove unused member declaration.

        public override void Initialize(int id, int componentCount, IEntityManager manager)
        {
            base.Initialize(id, componentCount, manager);

            _gameId = Manager.Game.Id;
        }

        protected override void InitializeComponent(Component component)
        {
            base.InitializeComponent(component);

            var gameComponent = (GameComponent)component;
            gameComponent.GameId = GameId;
            gameComponent.EntityId = Id;
        }

        protected override string GetComponentPropertyName(int componentId)
        {
            var name = Manager.GetComponentPropertyName(componentId);
            Debug.Assert(name != null);

            return name;
        }

        public TComponent GetOrAddComponent<TComponent>(EntityComponent componentId)
            where TComponent : Component, new()
            => GetOrAddComponent<TComponent>((int)componentId);

        public void RemoveComponent(EntityComponent componentId)
            => RemoveComponent((int)componentId);

        public bool HasComponent(EntityComponent componentId)
            => HasComponent((int)componentId);

        public new OwnerTransientReference<GameEntity, TOwner> AddReference<TOwner>(TOwner owner)
            => new OwnerTransientReference<GameEntity, TOwner>(this, owner);

        private class RelationshipsView
        {
            public RelationshipsView(GameEntity entity) => Entity = entity;
            private GameEntity Entity { get; }

            public IReadOnlyCollection<GameEntity> AffectableAbilities
                => Entity.Manager.AbilitiesToAffectableRelationship[Entity.Id];

            public IReadOnlyCollection<GameEntity> AffectableAppliedEffects
                => Entity.Manager.AppliedEffectsToAffectableEntityRelationship[Entity.Id];

            public IReadOnlyCollection<GameEntity> AbilityAppliedEffects
                => Entity.Manager.AppliedEffectsToSourceAbilityRelationship[Entity.Id];

            public IReadOnlyCollection<GameEntity> AbilityEffects
                => Entity.Manager.EffectsToContainingAbilityRelationship[Entity.Id];

            public IEnumerable<GameEntity> Races
                => Entity.Manager.RacesToBeingRelationship[Entity.Id].Values;

            public IReadOnlyCollection<GameEntity> ContainerItems
                => Entity.Manager.EntityItemsToContainerRelationship[Entity.Id];

            public IReadOnlyCollection<GameEntity> Connections
                => Entity.Manager.ConnectionsToLevelRelationship[Entity.Id];

            public IReadOnlyCollection<GameEntity> IncomingConnections
                => Entity.Manager.IncomingConnectionsToLevelRelationship[Entity.Id];

            public IReadOnlyCollection<GameEntity> LevelActors
                => Entity.Manager.LevelActorsToLevelRelationship[Entity.Id];

            public IReadOnlyCollection<GameEntity> LevelItems
                => Entity.Manager.LevelItemsToLevelRelationship[Entity.Id];

            public IReadOnlyCollection<GameEntity> LevelKnowledges
                => Entity.Manager.LevelKnowledgesToLevelRelationship[Entity.Id];

            public GameEntity Knowledge => Entity.Manager.LevelKnowledgeToLevelEntityRelationship[Entity.Id];
        }
    }
}
