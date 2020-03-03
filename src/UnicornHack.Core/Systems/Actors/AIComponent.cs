using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Systems.Actors
{
    [Component(Id = (int)EntityComponent.AI)]
    public class AIComponent : GameComponent
    {
        private string _properName;
        private ActorAction? _nextAction;
        private int? _nextActionTarget;
        private int? _nextActionTarget2;
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

        public ActorAction? NextAction
        {
            get => _nextAction;
            set => SetWithNotify(value, ref _nextAction);
        }

        public int? NextActionTarget
        {
            get => _nextActionTarget;
            set => SetWithNotify(value, ref _nextActionTarget);
        }

        public int? NextActionTarget2
        {
            get => _nextActionTarget2;
            set => SetWithNotify(value, ref _nextActionTarget2);
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

        protected override void Clean()
        {
            _properName = default;
            _nextAction = default;
            _nextActionTarget = default;
            _nextActionTarget2 = default;
            _nextActionTick = default;
            _behavior = default;
            _noise = default;

            base.Clean();
        }
    }
}
