using UnicornHack.Generation;
using UnicornHack.Primitives;

namespace UnicornHack.Systems.Beings
{
    [Component(Id = (int)EntityComponent.Race)]
    public class RaceComponent : GameComponent
    {
        private string _templateName;
        private Species _species;
        private SpeciesClass _speciesClass;
        private byte _level;
        private int _experiencePoints;
        private int _nextLevelXP;

        public RaceComponent()
            => ComponentId = (int)EntityComponent.Race;

        public string TemplateName
        {
            get => _templateName;
            set => SetWithNotify(value, ref _templateName);
        }

        public Species Species
        {
            get => _species;
            set => SetWithNotify(value, ref _species);
        }

        public SpeciesClass SpeciesClass
        {
            get => _speciesClass;
            set => SetWithNotify(value, ref _speciesClass);
        }

        public byte Level
        {
            get => _level;
            set => SetWithNotify(value, ref _level);
        }

        public int ExperiencePoints
        {
            get => _experiencePoints;
            set => SetWithNotify(value, ref _experiencePoints);
        }

        public int NextLevelXP
        {
            get => _nextLevelXP;
            set => SetWithNotify(value, ref _nextLevelXP);
        }

        protected override void Clean()
        {
            _templateName = default;
            _species = default;
            _speciesClass = default;
            _level = default;
            _experiencePoints = default;
            _nextLevelXP = default;

            base.Clean();
        }
    }
}
