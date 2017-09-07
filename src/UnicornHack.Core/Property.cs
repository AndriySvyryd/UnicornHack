namespace UnicornHack
{
    public abstract class Property
    {
        protected Property()
        {
        }

        protected Property(Entity entity, string name)
        {
            Game = entity.Game;
            Name = name;
            Entity = entity;
        }

        public string Name { get; set; }
        public abstract object Value { get; }

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }

        public int EntityId { get; private set; }

        public Game Game { get; set; }
        public Entity Entity { get; set; }

        public abstract void UpdateValue();
    }
}