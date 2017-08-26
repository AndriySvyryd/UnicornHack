using System;
using System.Collections.Generic;
using UnicornHack.Effects;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Entity
    {
        public string Name { get; set; }
        public string BaseName { get; set; }
        public virtual Material Material { get; set; }
        public virtual ISet<Ability> Abilities { get; set; } = new HashSet<Ability>();

        public ISet<AppliedEffect> ActiveEffects { get; set; } = new HashSet<AppliedEffect>();

        public SortedListAdapter<string, Property> Properties { get; set; } = new SortedListAdapter<string, Property>(
            new SortedList<string, Property>(StringComparer.Ordinal), e => e.Name);

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int Id { get; private set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public int GameId { get; private set; }

        public Game Game { get; set; }

        public Entity()
        {
        }

        public Entity(Game game) : this()
        {
            Game = game;
            Id = game.NextEntityId++;
            game.Entities.Add(this);
        }

        public T GetProperty<T>(string propertyName)
        {
            if (Properties.List.TryGetValue(propertyName, out var property))
            {
                if (!property.IsCurrent)
                {
                    property.UpdateValue();
                }
                return ((Property<T>)property).TypedValue;
            }

            return ((PropertyDescription<T>)PropertyDescription.Loader.Get(propertyName)).DefaultValue;
        }

        public void InvalidateProperty<T>(string propertyName)
        {
            if (!Properties.List.TryGetValue(propertyName, out var property))
            {
                property = new Property<T>(this, propertyName);
                Properties.Add(property);
            }

            property.IsCurrent = false;
        }

        public virtual void Add(Ability ability)
        {
            Abilities.Add(ability.AddReference().Referenced);
            ability.Entity = this;
            (this as IReferenceable)?.AddReference();
            // TODO: Trigger permanent abilities here
        }

        public virtual void Remove(Ability ability)
        {
            Abilities.Remove(ability);
            ability.RemoveReference();
            ability.Entity = null;
            (this as IReferenceable)?.RemoveReference();
        }
    }
}