using UnicornHack.Events;

namespace UnicornHack.Effects
{
    public abstract class Effect
    {
        protected Effect()
        {
        }

        protected Effect(Game game)
        {
            Game = game;
            Id = game.NextEffectId++;
            game.Effects.Add(this);
        }

        public int Id { get; private set; }
        public int GameId { get; private set; }
        public Game Game { get; set; }

        public abstract Effect Instantiate(Game game);

        public abstract void Apply(AbilityActivationContext abilityContext);
    }
}