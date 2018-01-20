namespace UnicornHack.Effects
{
    public abstract class DurationEffect : Effect
    {
        protected DurationEffect()
        {
        }

        protected DurationEffect(Game game) : base(game)
        {
        }

        protected DurationEffect(DurationEffect effect, Game game)
            : base(effect, game)
            => Duration = effect.Duration;

        public int Duration { get; set; }
    }
}