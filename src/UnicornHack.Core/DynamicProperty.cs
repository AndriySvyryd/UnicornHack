namespace UnicornHack
{
    public class DynamicProperty<T> : Property<T>
    {
        public DynamicProperty()
        {
        }

        public DynamicProperty(Entity entity, string name) : base(entity, name)
            => _currentValue = LastValue;

        private T _currentValue;

        public override T CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                UpdateValue();
            }
        }

        public override void UpdateValue()
        {
            Entity.OnPropertyChanged(Name, LastValue, CurrentValue);

            LastValue = CurrentValue;
        }
    }
}