using UnicornHack.Generation;

namespace UnicornHack.Systems.Senses
{
    [Component(Id = (int)EntityComponent.Sensor)]
    public class SensorComponent : GameComponent
    {
        private int _primaryFOVQuadrants;
        private int _totalFOVQuadrants;
        private int _primaryVisionRange;
        private int _secondaryVisionRange;
        private bool _infravision;
        private bool _invisibilityDetection;

        public SensorComponent()
            => ComponentId = (int)EntityComponent.Sensor;

        [Property(IsCalculated = true, DefaultValue = 1)]
        public int PrimaryFOVQuadrants
        {
            get => _primaryFOVQuadrants;
            set => SetWithNotify(value, ref _primaryFOVQuadrants);
        }

        [Property(IsCalculated = true, DefaultValue = 2)]
        public int TotalFOVQuadrants
        {
            get => _totalFOVQuadrants;
            set => SetWithNotify(value, ref _totalFOVQuadrants);
        }

        [Property(IsCalculated = true, DefaultValue = 16)]
        public int PrimaryVisionRange
        {
            get => _primaryVisionRange;
            set => SetWithNotify(value, ref _primaryVisionRange);
        }

        [Property(IsCalculated = true, DefaultValue = 8)]
        public int SecondaryVisionRange
        {
            get => _secondaryVisionRange;
            set => SetWithNotify(value, ref _secondaryVisionRange);
        }

        [Property(IsCalculated = true)]
        public bool Infravision
        {
            get => _infravision;
            set => SetWithNotify(value, ref _infravision);
        }

        [Property(IsCalculated = true)]
        public bool InvisibilityDetection
        {
            get => _invisibilityDetection;
            set => SetWithNotify(value, ref _invisibilityDetection);
        }

        // Untracked properties
        public byte[] VisibleTerrain { get; set; }
        public bool VisibleTerrainIsCurrent { get; set; }
    }
}
