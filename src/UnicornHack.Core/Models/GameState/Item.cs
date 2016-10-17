using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameState
{
    public class Item : IReferenceable
    {
        // For EF
        protected Item()
        {
        }

        public Item(ItemType type, Game game)
        {
            Type = type;
            Game = game;
            Id = game.NextItemId++;
            game.Items.Add(this);
        }

        public int Id { get; private set; }
        public string Name => GivenName ?? GetName(Type);
        public string GivenName { get; set; }
        public ItemType Type { get; private set; }

        // One and only one of theese shouldn't be null
        public int? ActorId { get; set; }
        public Actor Actor { get; set; }
        public int? ContainerId { get; set; }
        public Container Container { get; set; }
        public int? LevelId { get; set; }
        public Level Level { get; set; }
        public byte? LevelX { get; set; }
        public byte? LevelY { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }
        public Game Game { get; set; }

        public virtual bool MoveTo(Level level, byte x, byte y)
        {
            if (!level.CanAdd(this, x, y))
            {
                return false;
            }

            using (AddReference())
            {
                Remove();
                level.TryAdd(this, x, y);
            }

            return true;
        }

        public virtual bool MoveTo(Actor actor)
        {
            if (!actor.CanAdd(this))
            {
                return false;
            }

            using (AddReference())
            {
                Remove();
                actor.TryAdd(this);
            }

            return true;
        }

        public virtual bool MoveTo(Container container)
        {
            if (!container.CanAdd(this))
            {
                return false;
            }

            using (AddReference())
            {
                Remove();
                container.TryAdd(this);
            }

            return true;
        }

        protected virtual void ReplaceWith(Item item)
        {
            if (Container != null)
            {
                var container = Container;
                Container.Remove(this);
                var added = container.TryAdd(item);
                Debug.Assert(added);
            }

            if (Actor != null)
            {
                var actor = Actor;
                Actor.Remove(this);
                var added = actor.TryAdd(item);
                Debug.Assert(added);
            }

            if (Level != null)
            {
                var level = Level;
                var levelX = LevelX;
                var levelY = LevelY;
                Level.Remove(this);
                var added = level.TryAdd(item, levelX.Value, levelY.Value);
                Debug.Assert(added);
            }

            Debug.Assert(_referenceCount == 0);
        }

        public virtual Item StackWith(IEnumerable<Item> existingItems)
        {
            foreach (var existingItem in existingItems)
            {
                if (existingItem.Type == Type)
                {
                    var stack = existingItem as ItemStack;
                    if (stack == null)
                    {
                        stack = new ItemStack(Type, Game);

                        existingItem.MoveTo(stack);

                        stack.TryAdd(this);
                        return stack;
                    }

                    if (!stack.CanAdd(this))
                    {
                        continue;
                    }

                    stack.TryAdd(this);
                    return null;
                }
            }

            return this;
        }

        protected void Remove()
        {
            Actor?.Remove(this);

            Container?.Remove(this);

            Level?.Remove(this);
        }

        public virtual TransientReference<Item> Split(int quantity)
        {
            if (quantity != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(quantity));
            }

            var result = new TransientReference<Item>(this);

            Remove();

            return result;
        }

        private string GetName(ItemType type)
        {
            switch (type)
            {
                case ItemType.Gold:
                    return "gold";
                case ItemType.Food:
                    return "carrot";
                default:
                    throw new NotSupportedException($"Item type {type} not supported");
            }
        }

        private int _referenceCount;
        void IReferenceable.AddReference()
        {
            _referenceCount++;
        }

        public TransientReference<Item> AddReference()
        {
            return new TransientReference<Item>(this);
        }

        public void RemoveReference()
        {
            if(--_referenceCount == 0)
            {
                Game.Delete(this);
            }
        }
    }
}