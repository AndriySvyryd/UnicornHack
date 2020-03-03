using UnicornHack.Systems.Beings;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Knowledge
{
    public class LeveledUpMessage : IMessage
    {
        public const string Name = "LeveledUp";

        public static LeveledUpMessage Create(GameManager manager)
            => manager.Queue.CreateMessage<LeveledUpMessage>(Name);

        private GameEntity _entity;
        private RaceComponent _race;

        public GameEntity Entity
        {
            get => _entity;
            set
            {
                _entity?.RemoveReference(this);
                _entity = value;
                _entity?.AddReference(this);
            }
        }

        public RaceComponent Race
        {
            get => _race;
            set
            {
                (_race as IOwnerReferenceable)?.RemoveReference(this);
                _race = value;
                (_race as IOwnerReferenceable)?.AddReference(this);
            }
        }

        public int SkillPointsGained { get; set; }
        public int TraitPointsGained { get; set; }
        public int MutationPointsGained { get; set; }

        string IMessage.MessageName { get; set; }

        public void Clean()
        {
            Entity = default;
            Race = default;
            SkillPointsGained = default;
            TraitPointsGained = default;
            MutationPointsGained = default;
        }
    }
}
