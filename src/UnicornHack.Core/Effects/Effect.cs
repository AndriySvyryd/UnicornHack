using System.Linq;
using System.Reflection;
using UnicornHack.Abilities;

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

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; private set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }

        public Game Game { get; set; }

        // TODO: Abstract application condition
        // TODO: Create abstract base for duration effects
        public int? Duration { get; set; }

        public abstract Effect Copy(Game game);

        public static Effect CreateChangeProperty(Game game, string propertyName, object value)
            => (Effect)_createChangeProperty
                .MakeGenericMethod(PropertyDescription.Loader.Get(propertyName).PropertyType)
                .Invoke(null, new[] {game, propertyName, value});

        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo _createChangeProperty = typeof(Effect).GetTypeInfo()
            .GetDeclaredMethods(nameof(CreateChangeProperty)).Single(mi => mi.IsGenericMethodDefinition);

        private static Effect CreateChangeProperty<T>(Game game, string propertyName, T value)
            => new ChangeProperty<T>(game) {PropertyName = propertyName, Value = value};

        public abstract void Apply(AbilityActivationContext abilityContext);

        public virtual void Delete()
        {
            Game.Repository.Delete(this);
        }
    }
}