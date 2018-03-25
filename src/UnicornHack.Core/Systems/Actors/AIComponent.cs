using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Systems.Actors
{
    [Component(Id = (int)EntityComponent.AI)]
    public class AIComponent : GameComponent
    {
        private string _properName;
        private int? _nextActionTick;
        private AIBehavior _behavior;
        private ActorNoiseType _noise;

        public AIComponent()
            => ComponentId = (int)EntityComponent.AI;

        public string ProperName
        {
            get => _properName;
            set => SetWithNotify(value, ref _properName);
        }

        public int? NextActionTick
        {
            get => _nextActionTick;
            set => SetWithNotify(value, ref _nextActionTick);
        }

        public AIBehavior Behavior
        {
            get => _behavior;
            set => SetWithNotify(value, ref _behavior);
        }

        public ActorNoiseType Noise
        {
            get => _noise;
            set => SetWithNotify(value, ref _noise);
        }
    }
}
