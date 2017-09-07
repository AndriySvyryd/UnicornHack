using System;

namespace UnicornHack.Effects
{
    public abstract class ChangedProperty : AppliedEffect
    {
        protected ChangedProperty()
        {
        }

        protected ChangedProperty(AbilityActivationContext abilityContext) : base(abilityContext)
        {
        }

        public string PropertyName { get; set; }
        public ValueCombinationFunction Function { get; set; }

        public static ChangedProperty<T> Create<T>(AbilityActivationContext abilityContext)
        {
            switch (typeof(T))
            {
                case var type when type == typeof(int):
                    return (ChangedProperty<T>)(object)new ChangedIntProperty(abilityContext);
                case var type when type == typeof(bool):
                    return (ChangedProperty<T>)(object)new ChangedBoolProperty(abilityContext);
                default:
                    throw new InvalidOperationException(nameof(ChangedProperty) + " for type " + typeof(T).Name +
                                                        " not implemented.");
            }
        }

        public abstract void UpdateProperty();

        public override void Remove()
        {
            base.Remove();

            UpdateProperty();
        }
    }
}