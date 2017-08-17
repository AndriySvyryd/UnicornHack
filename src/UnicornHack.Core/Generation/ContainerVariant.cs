using System;
using System.Collections.Generic;
using CSharpScriptSerialization;

namespace UnicornHack.Generation
{
    public class ContainerVariant : ItemVariant
    {
        private short? _capacity;
        public virtual short Capacity
        {
            get => _capacity ?? (Base as ContainerVariant)?.Capacity ?? Int16.MaxValue;
            set => _capacity = value;
        }

        public override Item Instantiate(Game game)
        {
            var container = (Container)base.Instantiate(game);
            container.Capacity = Capacity;
            return container;
        }

        protected override Item CreateInstance(Game game) => new Container(game);

        private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<ContainerVariant>(
            GetPropertyConditions<ContainerVariant>());

        protected new static Dictionary<string, Func<TItemVariant, object, bool>> GetPropertyConditions<TItemVariant>()
            where TItemVariant : ContainerVariant
        {
            var propertyConditions = ItemVariant.GetPropertyConditions<TItemVariant>();
            propertyConditions.Add(nameof(Capacity), (o, v) => (short)v != Int16.MaxValue);
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}