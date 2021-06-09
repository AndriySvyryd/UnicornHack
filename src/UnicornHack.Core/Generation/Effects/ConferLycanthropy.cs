using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Primitives;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Generation.Effects
{
    public class ConferLycanthropy : Effect
    {
        public string VariantName { get; set; }

        protected override void ConfigureEffect(EffectComponent effect)
        {
            effect.EffectType = EffectType.ConferLycanthropy;
            effect.TargetName = VariantName;
        }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<ConferLycanthropy>(GetPropertyConditions<ConferLycanthropy>());

        protected static new Dictionary<string, Func<TEffect, object, bool>>
            GetPropertyConditions<TEffect>() where TEffect : Effect
        {
            var propertyConditions = Effect.GetPropertyConditions<TEffect>();

            propertyConditions.Add(nameof(VariantName), (o, v) => v != default);
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
