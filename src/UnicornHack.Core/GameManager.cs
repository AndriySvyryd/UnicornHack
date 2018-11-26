using UnicornHack.Utils.MessagingECS;

namespace UnicornHack
{
    public partial class GameManager : EntityManager<GameEntity>, IGameSystem<RemoveComponentMessage>
    {
        public const string RemoveComponentMessageName = "RemoveComponent";

        private readonly string[] _componentPropertyNames;

        public GameManager()
            : base((int)EntityComponent.ComponentCount, entityPoolSize: 32)
            => _componentPropertyNames = new string[(int)EntityComponent.ComponentCount];

        public Game Game { get; set; }
        public new SequentialMessageQueue<GameManager> Queue => (SequentialMessageQueue<GameManager>)base.Queue;

        protected override void InitializeSystems(IMessageQueue queue)
        {
            var gameQueue = (SequentialMessageQueue<GameManager>)queue;
            gameQueue.Add(this, RemoveComponentMessageName, 0);

            InitializeLevels(gameQueue);
            InitializeActors(gameQueue);
            InitializeBeings(gameQueue);
            InitializeAbilities(gameQueue);
            InitializeItems(gameQueue);
            InitializeFaculties(gameQueue);
            InitializeSenses(gameQueue);
            InitializeEffects(gameQueue);
            InitializeKnowledge(gameQueue);
            InitializeTime(gameQueue);
        }

        public override int GetNextEntityId() => ++Game.NextEntityId;

        protected override GameEntity CreateEntityNoReference()
        {
            var entity = base.CreateEntityNoReference();
            Game.Repository.Add(entity);
            return entity;
        }

        public RemoveComponentMessage CreateRemoveComponentMessage()
            => Queue.CreateMessage<RemoveComponentMessage>(RemoveComponentMessageName);

        public TComponent CreateComponent<TComponent>(EntityComponent componentId)
            where TComponent : GameComponent, new()
            => CreateComponent<TComponent>((int)componentId);

        protected void Add<TComponent>(EntityComponent componentId, int poolSize)
            where TComponent : Component, new()
            => Add<TComponent>((int)componentId, poolSize);

        protected override void Add<TComponent>(int componentId, int poolSize)
        {
            _componentPropertyNames[componentId] = ((EntityComponent)componentId).ToString();
            base.Add<TComponent>(componentId, poolSize);
        }

        public string GetComponentPropertyName(int componentId)
            => _componentPropertyNames[componentId];

        public GameEntity FindEntity(int? id) => id.HasValue ? base.FindEntity(id.Value) : null;
        public override Entity LoadEntity(int id) => Game.Repository.Find<GameEntity>(id);

        public override void RemoveFromSecondaryTracker(ITrackable trackable)
            => Game.Repository.RemoveTracked(trackable);

        public MessageProcessingResult Process(RemoveComponentMessage message, GameManager manager)
        {
            message.Entity.RemoveComponent(message.Component);

            return MessageProcessingResult.ContinueProcessing;
        }
    }
}
