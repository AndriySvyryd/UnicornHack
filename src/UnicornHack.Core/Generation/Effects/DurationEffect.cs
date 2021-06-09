using System;
using System.Collections.Generic;
using System.Diagnostics;
using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public abstract class DurationEffect : Effect
    {
        private Func<GameEntity, GameEntity, float> _durationFunction;
        public EffectDuration Duration { get; set; }
        public string DurationAmount { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            Debug.Assert(DurationAmount == null
                         || Duration == EffectDuration.UntilTimeout
                         || Duration == EffectDuration.UntilXPGained);

            effect.Duration = Duration;
            effect.DurationAmount = DurationAmount;

            if (DurationAmount != null)
            {
                if (_durationFunction == null)
                {
                    _durationFunction = EffectApplicationSystem.CreateAmountFunction(DurationAmount, ContainingAbility.Name);
                }
                effect.DurationAmountFunction = _durationFunction;
            }
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<DurationEffect>(GetPropertyConditions<DurationEffect>());

        protected static new Dictionary<string, Func<TEffect, object, bool>>
            GetPropertyConditions<TEffect>() where TEffect : Effect
        {
            var propertyConditions = Effect.GetPropertyConditions<TEffect>();

            propertyConditions.Add(nameof(Duration), (o, v) => (EffectDuration)v != default);
            propertyConditions.Add(nameof(DurationAmount), (o, v) => v != default);
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
