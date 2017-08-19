using System.Collections.Generic;
using System.Linq;

namespace UnicornHack
{
    public class LevelCell : IItemLocation
    {
        private readonly Level _level;
        private readonly byte _x;
        private readonly byte _y;

        public LevelCell(Level level, byte x, byte y)
        {
            _level = level;
            _x = x;
            _y = y;
        }

        public Game Game => _level.Game;

        public IEnumerable<Item> Items
        {
            get { return _level.Items.Where(i => i.LevelX == _x && i.LevelY == _y); }
        }

        public bool TryAdd(Item item) => _level.TryAdd(item, _x, _y);

        public bool CanAdd(Item item) => _level.CanAdd(item, _x, _y);
    }
}