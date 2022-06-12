using UnicornHack.Generation;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Levels
{
    [Component(Id = (int)EntityComponent.Connection)]
    public class ConnectionComponent : GameComponent
    {
        private GameEntity _targetLevelEntity;
        private int _targetLevelId;
        private byte? _targetLevelX;
        private byte? _targetLevelY;
        private ConnectionDirection? _connectionDirection;

        public ConnectionComponent()
            => ComponentId = (int)EntityComponent.Connection;

        public int TargetLevelId
        {
            get => _targetLevelId;
            set
            {
                _targetLevelEntity = null;
                SetWithNotify(value, ref _targetLevelId);
            }
        }

        public GameEntity TargetLevelEntity
        {
            get => _targetLevelEntity ??= Entity.Manager.FindEntity(_targetLevelId);
            set
            {
                _targetLevelId = value.Id;
                _targetLevelEntity = value;
            }
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
            get => _targetLevelX == null || _targetLevelY == null
                ? null
                : new Point(_targetLevelX.Value, _targetLevelY.Value);
            set
            {
                var levelXSet = NotifyChanging(value?.X, ref _targetLevelX, nameof(TargetLevelX), out var oldLevelX);
                var levelYSet = NotifyChanging(value?.Y, ref _targetLevelY, nameof(TargetLevelY), out var oldLevelY);

                var anyChanges = false;
                var levelXChange = PropertyValueChange<byte?>.Empty;
                if (levelXSet)
                {
                    anyChanges = true;
                    NotifyChanged(nameof(TargetLevelX));
                    levelXChange = new PropertyValueChange<byte?>(this, nameof(TargetLevelX), oldLevelX, value?.X);
                }

                var levelYChange = PropertyValueChange<byte?>.Empty;
                if (levelYSet)
                {
                    anyChanges = true;
                    NotifyChanged(nameof(TargetLevelY));
                    levelYChange = new PropertyValueChange<byte?>(this, nameof(TargetLevelY), oldLevelY, value?.Y);
                }

                if (anyChanges)
                {
                    Entity?.HandlePropertyValuesChanged(IPropertyValueChanges.Create(levelXChange, levelYChange));
                }
            }
        }

        public ConnectionDirection? Direction
        {
            get => _connectionDirection;
            set => SetWithNotify(value, ref _connectionDirection);
        }

        protected override void Clean()
        {
            _targetLevelEntity = default;
            _targetLevelId = default;
            _targetLevelX = default;
            _targetLevelY = default;
            _connectionDirection = default;

            base.Clean();
        }
    }
}
