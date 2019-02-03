using System;
using System.Collections.Generic;
using CSharpScriptSerialization;
using UnicornHack.Primitives;

namespace UnicornHack.Generation
{
    public class WieldingAbility : Ability
    {
        public WieldingStyle? WieldingStyle { get; set; }
        public ItemType? ItemType { get; set; }
        public EffectType? DamageType { get; set; }

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<WieldingAbility>(GetPropertyConditions<WieldingAbility>());

        protected new static Dictionary<string, Func<TAbility, object, bool>>
            GetPropertyConditions<TAbility>() where TAbility : Ability
        {
            var propertyConditions = Ability.GetPropertyConditions<TAbility>();

            propertyConditions.Add(nameof(WieldingStyle), (o, v) => v != default);
            propertyConditions.Add(nameof(ItemType), (o, v) => v != default);
            propertyConditions.Add(nameof(DamageType), (o, v) => v != default);
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}
