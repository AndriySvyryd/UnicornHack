using UnicornHack.Generation;

namespace UnicornHack.Systems.Knowledge
{
    [Component(Id = (int)EntityComponent.Knowledge)]
    public class KnowledgeComponent : GameComponent
    {
        private GameEntity _knownEntity;
        private int _knownEntityId;

        public KnowledgeComponent()
            => ComponentId = (int)EntityComponent.Knowledge;

        public int KnownEntityId
        {
            get => _knownEntityId;
            set => SetWithNotify(value, ref _knownEntityId);
        }

        // TODO: Perception level

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
    }
}
