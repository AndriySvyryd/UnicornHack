using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Utils;

namespace UnicornHack
{
    public abstract class Entity : IReferenceable
    {
        public string Name { get; set; }
        public string VariantName { get; set; }
        public virtual Material Material { get; set; }
        public virtual ObservableSnapshotHashSet<Ability> Abilities { get; set; } = new ObservableSnapshotHashSet<Ability>();
        public ObservableSnapshotHashSet<AppliedEffect> ActiveEffects { get; set; } = new ObservableSnapshotHashSet<AppliedEffect>();

        public SortedListAdapter<string, Property> Properties { get; set; } = new SortedListAdapter<string, Property>(
            new SortedList<string, Property>(StringComparer.Ordinal), e => e.Name);

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int Id { get; private set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }

        public Game Game { get; set; }

        public string BranchName { get; set; }
        public byte? LevelDepth { get; set; }
        public Level Level { get; set; }
        public byte LevelX { get; set; }
        public byte LevelY { get; set; }

        public Point LevelCell => new Point(LevelX, LevelY);

        protected int AbilitiesBeingActivated { get; set; }

        private static readonly Dictionary<string, List<object>> PropertyListeners = new Dictionary<string, List<object>>();

        protected Entity()
        {
        }

        protected Entity(Game game) : this()
        {
            Game = game;
            Id = ++game.NextEntityId;
            game.Entities.Add(this);
        }

        private int _referenceCount;

        void IReferenceable.AddReference()
        {
            _referenceCount++;
        }

        public void RemoveReference()
        {
            if (--_referenceCount <= 0)
            {
                Delete();
            }
        }

        protected virtual void Delete()
            => Game.Repository.Delete(this);

        public T GetProperty<T>(string propertyName)
            => Properties.List.TryGetValue(propertyName, out var property)
                ? ((Property<T>)property).CurrentValue
                : ((PropertyDescription<T>)PropertyDescription.Loader.Get(propertyName)).DefaultValue;

        public Property<T> InvalidateProperty<T>(string propertyName)
        {
            if (PropertyDescription.Loader.Get(propertyName).IsCalculated)
            {
                if (!Properties.List.TryGetValue(propertyName, out var property))
                {
                    property = new CalculatedProperty<T>(this, propertyName);
                    Properties.Add(property);
                }

                var calculatedProperty = (CalculatedProperty<T>)property;

                calculatedProperty.IsCurrent = false;

                if (HasListener(propertyName))
                {
                    property.UpdateValue();
                }

                return calculatedProperty;
            }
            else
            {
                if (!Properties.List.TryGetValue(propertyName, out var property))
                {
                    property = new DynamicProperty<T>(this, propertyName);
                    Properties.Add(property);
                }

                return (Property<T>)property;
            }
        }

        public virtual void OnPropertyChanged<T>(string propertyName, T oldValue, T newValue)
        {
            if (PropertyListeners.TryGetValue(propertyName, out var listeners))
            {
                foreach (var listener in listeners)
                {
                    ((Action<Entity, T, T>)listener)(this, oldValue, newValue);
                }
            }
        }

        protected virtual bool HasListener(string propertyName)
            => PropertyListeners.ContainsKey(propertyName);

        private static void AddPropertyListener<T>(string propertyName, Action<Entity, T, T> action)
        {
            if (!PropertyListeners.TryGetValue(propertyName, out var listeners))
            {
                listeners = new List<object>();
                PropertyListeners[propertyName] = listeners;
            }

            listeners.Add(action);
        }

        public abstract void UpdateKnownPosition();

        public virtual void ActivateAbilities(AbilityActivation activation, Entity activator, Entity target)
        {
            using (var context = new AbilityActivationContext
            {
                Activator = activator,
                TargetEntity = target
            })
            {
                ActivateAbilities(activation, context, useSameContext: false);
            }
        }

        public virtual void ActivateAbilities(
            AbilityActivation activation, AbilityActivationContext context, bool useSameContext)
        {
            foreach (var ability in Abilities.Where(a => a.Activation == activation && !a.IsActive).ToList())
            {
                if (ability.Action == AbilityAction.Modifier
                    || useSameContext)
                {
                    ability.Activate(context);
                }
                else
                {
                    using (var newContext = new AbilityActivationContext
                    {
                        Activator = context.Activator,
                        TargetEntity = context.TargetEntity,
                        TargetCell = context.TargetCell
                    })
                    {
                        ability.Activate(newContext);
                    }
                }
            }
        }

        public virtual void Add(Ability ability)
        {
            Abilities.Add(ability.AddReference().Referenced);
            ability.Entity = this;

            ActivateAbilities(AbilityActivation.Always, this, this);
        }

        public virtual void Remove(Ability ability)
        {
            Abilities.Remove(ability);
            ability.RemoveReference();
            ability.Entity = null;

            if (ability.IsActive)
            {
                ability.Deactivate();
            }
        }

        public virtual void OnAbilityActivating(Ability ability)
        {
            AbilitiesBeingActivated++;
        }

        public virtual void OnAbilityActivated(Ability ability)
        {
            AbilitiesBeingActivated--;
        }
    }
}