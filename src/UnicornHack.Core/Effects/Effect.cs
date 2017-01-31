using UnicornHack.Events;
using UnicornHack.Utils;

namespace UnicornHack.Effects
{
    public abstract class Effect : IReferenceable
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

        private int _referenceCount;

        void IReferenceable.AddReference()
        {
            _referenceCount++;
        }

        public TransientReference<Effect> AddReference()
        {
            return new TransientReference<Effect>(this);
        }

        public void RemoveReference()
        {
            if (--_referenceCount < 0)
            {
                Delete();
            }
        }

        protected virtual void Delete()
        {
            Game.Delete(this);
        }

        public abstract void Apply(AbilityActivationContext abilityContext);
    }
}