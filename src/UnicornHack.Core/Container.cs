using System;
using System.Collections.Generic;
using System.Linq;
using CSharpScriptSerialization;

namespace UnicornHack
{
    public class Container : Item, IItemLocation
    {
        private short? _capacity;

        public Container()
        {
        }

        public Container(Game game)
            : base(game)
        {
        }

        public virtual short Capacity
        {
            get { return _capacity ?? (BaseItem as Container)?.Capacity ?? Int16.MaxValue; }
            set { _capacity = value; }
        }

        public override int Weight => Items.Sum(i => i.Weight);
        public ICollection<Item> Items { get; } = new HashSet<Item>();
        IEnumerable<Item> IItemLocation.Items => Items;
        public virtual int Quantity => Items.Count;
        protected virtual bool CanContainStacks => true;

        public override Item Instantiate(Game game)
        {
            var container = (Container)base.Instantiate(game);
            container.Capacity = Capacity;
            return container;
        }

        protected override Item CreateInstance(Game game) => new Container(game);

        public virtual bool TryAdd(Item item)
        {
            if (!CanAdd(item))
            {
                return false;
            }

            var itemOrStack = CanContainStacks ? item.StackWith(Items) : item;
            if (itemOrStack != null)
            {
                itemOrStack.ContainerId = Id;
                itemOrStack.Container = this;
                Items.Add(itemOrStack);
                itemOrStack.AddReference();
            }

            return true;
        }

        public virtual bool CanAdd(Item item)
            => Quantity < Capacity
               && (!(item is Container)
                   || CanContainStacks && item is ItemStack);

        public virtual bool Remove(Item item)
        {
            item.ContainerId = null;
            item.Container = null;
            if (Items.Remove(item))
            {
                item.RemoveReference();
                return true;
            }
            return false;
        }

        private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<Container>(
            GetPropertyConditions<Container>());

        protected new static Dictionary<string, Func<TItem, object, bool>> GetPropertyConditions<TItem>()
            where TItem : Container
        {
            var propertyConditions = Item.GetPropertyConditions<TItem>();
            propertyConditions.Add(nameof(Capacity), (o, v) => (short)v != Int16.MaxValue);
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}