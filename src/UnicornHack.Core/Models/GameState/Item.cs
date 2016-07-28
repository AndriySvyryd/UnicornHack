using System;
using UnicornHack.Models.GameDefinitions;

namespace UnicornHack.Models.GameState
{
    public class Item
    {
        protected Item()
        {
        }

        protected Item(ItemType type, Game game)
        {
            Id = game.NextItemId++;
            Game = game;
            Type = type;
            Name = GetName(type);
        }

        protected Item(ItemType type, Level level, byte x, byte y)
            : this(type, level.Game)
        {
            Level = level;
            LevelX = x;
            LevelY = y;
        }

        protected Item(ItemType type, Actor actor)
            : this(type, actor.Game)
        {
            Actor = actor;
        }

        public int Id { get; private set; }
        public string Name { get; set; }
        public ItemType Type { get; set; }
        public Actor Actor { get; set; }
        public byte LevelX { get; set; }
        public byte LevelY { get; set; }
        public Level Level { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }
        public Game Game { get; set; }

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
    }
}