using System.Linq;
using System.Reflection;
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

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; private set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }

        public Game Game { get; set; }

        public abstract Effect Instantiate(Game game);

        public static Effect CreateChangeValuedProperty(Game game, string propertyName, object value) =>
            (Effect)_createChangeValuedProperty
                .MakeGenericMethod(PropertyDescription.Loader.Get(propertyName).PropertyType)
                .Invoke(null, new[] {game, propertyName, value});

        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo _createChangeValuedProperty = typeof(Effect).GetTypeInfo()
            .GetDeclaredMethods(nameof(CreateChangeValuedProperty)).Single(mi => mi.IsGenericMethodDefinition);

        private static Effect CreateChangeValuedProperty<T>(Game game, string propertyName, T value) =>
            new ChangeProperty<T>(game) {PropertyName = propertyName, Value = value};

        private int _referenceCount;

        void IReferenceable.AddReference()
        {
            _referenceCount++;
        }

        public TransientReference<Effect> AddReference() => new TransientReference<Effect>(this);

        public void RemoveReference()
        {
            if (--_referenceCount < 0)
            {
                Delete();
            }
        }

        protected virtual void Delete()
        {
            Game.Repository.Delete(this);
        }

        public abstract void Apply(AbilityActivationContext abilityContext);
    }
}