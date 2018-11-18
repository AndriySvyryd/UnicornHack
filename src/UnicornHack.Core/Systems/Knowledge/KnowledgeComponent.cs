using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Systems.Knowledge
{
    [Component(Id = (int)EntityComponent.Knowledge)]
    public class KnowledgeComponent : GameComponent
    {
        private GameEntity _knownEntity;
        private int _knownEntityId;
        private SenseType _sensedType;

        public KnowledgeComponent()
            => ComponentId = (int)EntityComponent.Knowledge;

        public int KnownEntityId
        {
            get => _knownEntityId;
            set
            {
                SetWithNotify(value, ref _knownEntityId);
                _knownEntity = null;
            }
        }

        public SenseType SensedType
        {
            get => _sensedType;
            set => SetWithNotify(value, ref _sensedType);
        }

        // Unmapped properties

        public GameEntity KnownEntity
        {
            get => _knownEntity ?? (_knownEntity = Entity.Manager.FindEntity(_knownEntityId));
            set
            {
                KnownEntityId = value.Id;
                _knownEntity = value;
            }
        }

        protected override void Clean()
        {
            _knownEntity = default;
            _knownEntityId = default;
            _sensedType = default;

            base.Clean();
        }
    }
}
