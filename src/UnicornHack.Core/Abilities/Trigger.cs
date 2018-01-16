namespace UnicornHack.Abilities
{
    public abstract class Trigger
    {
        protected Trigger()
        {
        }

        protected Trigger(Game game)
        {
            Game = game;
            Id = ++game.NextTriggerId;
            game.Triggers.Add(this);
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; private set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }

        public Game Game { get; set; }
        public int? AbilityId { get; set; }
        public Ability Ability { get; set; }
        public int? AbilityDefinitionId { get; set; }

        public abstract Trigger Copy(Game game);

        public abstract void Fire(AbilityActivationContext abilityContext);

        public virtual void Delete()
        {
            Game.Repository.Delete(this);
        }
    }
}