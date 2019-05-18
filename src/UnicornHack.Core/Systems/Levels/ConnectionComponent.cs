using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Systems.Levels
{
    [Component(Id = (int)EntityComponent.Connection)]
    public class ConnectionComponent : GameComponent
    {
        private int _targetLevelId;
        private byte? _targetLevelX;
        private byte? _targetLevelY;
        private ConnectionDirection? _connectionDirection;

        public ConnectionComponent()
            => ComponentId = (int)EntityComponent.Connection;

        public int TargetLevelId
        {
            get => _targetLevelId;
            set => SetWithNotify(value, ref _targetLevelId);
        }

        public byte? TargetLevelX
        {
            get => _targetLevelX;
            set => SetWithNotify(value, ref _targetLevelX);
        }

        public byte? TargetLevelY
        {
            get => _targetLevelY;
            set => SetWithNotify(value, ref _targetLevelY);
        }

        public Point? TargetLevelCell
        {
            get => TargetLevelX == null || TargetLevelY == null
                ? (Point?)null
                : new Point(TargetLevelX.Value, TargetLevelY.Value);
            set
            {
                TargetLevelX = value?.X;
                TargetLevelY = value?.Y;
            }
        }

        public ConnectionDirection? Direction
        {
            get => _connectionDirection;
            set => SetWithNotify(value, ref _connectionDirection);
        }

        protected override void Clean()
        {
            _targetLevelId = default;
            _targetLevelX = default;
            _targetLevelY = default;
            _connectionDirection = default;

            base.Clean();
        }
    }
}
