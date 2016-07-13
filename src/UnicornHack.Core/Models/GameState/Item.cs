using System;
using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Item
    {
        public int Id { get; private set; }
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public Actor Actor { get; set; }
        public byte LevelX { get; set; }
        public byte LevelY { get; set; }
        public Level Level { get; set; }
        public Game Game { get; set; }

        public static Item CreateItem(ItemType type, byte x, byte y, Level level)
        {
            return CreateItem(CreateItem(type), x, y, level);
        }

        protected static Item CreateItem(Item item, byte x, byte y, Level level)
        {
            item.LevelX = x;
            item.LevelY = y;
            item.Level = level;
            item.Game = level.Game;
            level.Items.Add(item);
            return item;
        }

        public static Item CreateItem(ItemType type, Actor actor)
        {
            return CreateItem(CreateItem(type), actor);
        }

        protected static Item CreateItem(Item item, Actor actor)
        {
            item.Actor = actor;
            item.Game = actor.Game;
            actor.Inventory.Add(item);
            return item;
        }

        private static Item CreateItem(ItemType type)
        {
            switch (type)
            {
                default:
                    throw new NotSupportedException($"Item type {type} not supported");
            }
        }
    }
}