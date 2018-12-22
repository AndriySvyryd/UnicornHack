using System;
using System.Globalization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class ChangeProperty<T> : DurationEffect
        where T : IConvertible
    {
        public string PropertyName { get; set; }
        public T Value { get; set; }
        public ValueCombinationFunction Function { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            base.ConfigureEffect(effect);

            effect.EffectType = EffectType.ChangeProperty;
            effect.TargetName = PropertyName;
            effect.Amount = Value.ToInt32(CultureInfo.InvariantCulture);
            effect.Function = Function;
        }
    }
}
