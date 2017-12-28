using Microsoft.EntityFrameworkCore;
using UnicornHack.Abilities;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation;

namespace UnicornHack.Data
{
    // TODO: Extract visitor-like base class to handle inheritance better
    public static class EntityAttacher
    {
        public static void Attach(Level level, GameDbContext context)
        {
            var levelEntry = context.Entry(level);
            if (levelEntry.State != EntityState.Detached)
            {
                return;
            }

            levelEntry.State = EntityState.Unchanged;

            foreach (var actor in level.Actors)
            {
                Attach(actor, context);
            }

            foreach (var item in level.Items)
            {
                Attach(item, context);
            }

            foreach (var connection in level.Connections)
            {
                Attach(connection, context);
            }

            foreach (var room in level.Rooms)
            {
                Attach(room, context);
            }

            Attach(level.Branch, context);

            var randomEntry = levelEntry.Reference(l => l.GenerationRandom).TargetEntry;
            if (randomEntry.State != EntityState.Detached)
            {
                return;
            }

            randomEntry.State = EntityState.Unchanged;
        }

        public static void Attach(Game game, GameDbContext context)
        {
            var gameEntry = context.Entry(game);
            if (gameEntry.State != EntityState.Detached)
            {
                return;
            }

            gameEntry.State = EntityState.Unchanged;

            var randomEntry = gameEntry.Reference(g => g.Random).TargetEntry;
            if (randomEntry.State != EntityState.Detached)
            {
                return;
            }

            randomEntry.State = EntityState.Unchanged;
        }

        public static void Attach(Branch branch, GameDbContext context)
        {
            var branchEntry = context.Entry(branch);
            if (branchEntry.State != EntityState.Detached)
            {
                return;
            }

            branchEntry.State = EntityState.Unchanged;

            Attach(branch.Game, context);
        }

        public static void Attach(Connection connection, GameDbContext context)
        {
            var connectionEntry = context.Entry(connection);
            if (connectionEntry.State != EntityState.Detached)
            {
                return;
            }

            connectionEntry.State = EntityState.Unchanged;

            if (connection.TargetLevel != null)
            {
                Attach(connection.TargetLevel, context);
            }
        }

        public static void Attach(Room room, GameDbContext context)
        {
            var roomEntry = context.Entry(room);
            if (roomEntry.State != EntityState.Detached)
            {
                return;
            }

            roomEntry.State = EntityState.Unchanged;
        }

        public static void Attach(Actor actor, GameDbContext context)
        {
            var actorEntry = context.Entry(actor);
            if (actorEntry.State != EntityState.Detached)
            {
                return;
            }

            actorEntry.State = EntityState.Unchanged;

            foreach (var ability in actor.Abilities)
            {
                Attach(ability, context);
            }

            foreach (var effect in actor.ActiveEffects)
            {
                Attach(effect, context);
            }

            foreach (Property property in actor.Properties)
            {
                Attach(property, context);
            }

            foreach (var item in actor.Inventory)
            {
                Attach(item, context);
            }

            if (actor is Player player)
            {
                foreach (var logEntry in player.Log)
                {
                    Attach(logEntry, context);
                }

                foreach (var @event in player.SensedEvents)
                {
                    Attach(@event, context);
                }

                Attach(player.Skills, context);
            }
        }

        public static void Attach(LogEntry logEntry, GameDbContext context)
        {
            var logEntityEntry = context.Entry(logEntry);
            if (logEntityEntry.State != EntityState.Detached)
            {
                return;
            }

            logEntityEntry.State = EntityState.Unchanged;
        }

        public static void Attach(SensoryEvent @event, GameDbContext context)
        {
            var eventEntry = context.Entry(@event);
            if (eventEntry.State != EntityState.Detached)
            {
                return;
            }

            eventEntry.State = EntityState.Unchanged;

            if (@event is ActorMoveEvent actorMoveEvent)
            {
                Attach(actorMoveEvent.Mover, context);
                Attach(actorMoveEvent.Movee, context);
            }
            else if (@event is AttackEvent attackEvent)
            {
                Attach(attackEvent.Attacker, context);
                Attach(attackEvent.Victim, context);
                foreach (var appliedEffect in attackEvent.AppliedEffects)
                {
                    Attach(appliedEffect, context);
                }
            }
            else if (@event is DeathEvent deathEvent)
            {
                Attach(deathEvent.Corpse, context);
                Attach(deathEvent.Deceased, context);
            }
            else if (@event is ItemConsumptionEvent itemConsumptionEvent)
            {
                Attach(itemConsumptionEvent.Item, context);
                Attach(itemConsumptionEvent.Consumer, context);
            }
            else if (@event is ItemDropEvent itemDropEvent)
            {
                Attach(itemDropEvent.Item, context);
                Attach(itemDropEvent.Dropper, context);
            }
            else if (@event is ItemPickUpEvent itemPickUpEvent)
            {
                Attach(itemPickUpEvent.Item, context);
                Attach(itemPickUpEvent.Picker, context);
            }
            else if (@event is ItemEquipmentEvent itemEquipmentEvent)
            {
                Attach(itemEquipmentEvent.Item, context);
                Attach(itemEquipmentEvent.Equipper, context);
            }
            else if (@event is ItemUnequipmentEvent itemUnequipmentEvent)
            {
                Attach(itemUnequipmentEvent.Item, context);
                Attach(itemUnequipmentEvent.Unequipper, context);
            }
        }

        public static void Attach(Skills skills, GameDbContext context)
        {
            var skillsEntry = context.Entry(skills);
            if (skillsEntry.State != EntityState.Detached)
            {
                return;
            }

            skillsEntry.State = EntityState.Unchanged;
        }

        public static void Attach(Property property, GameDbContext context)
        {
            var propertyEntry = context.Entry(property);
            if (propertyEntry.State != EntityState.Detached)
            {
                return;
            }

            propertyEntry.State = EntityState.Unchanged;
        }

        public static void Attach(Ability ability, GameDbContext context)
        {
            var abilityEntry = context.Entry(ability);
            if (abilityEntry.State != EntityState.Detached)
            {
                return;
            }

            abilityEntry.State = EntityState.Unchanged;

            foreach (var effect in ability.Effects)
            {
                Attach(effect, context);
            }

            foreach (var trigger in ability.Triggers)
            {
                Attach(trigger, context);
            }
        }

        public static void Attach(AbilityDefinition ability, GameDbContext context)
        {
            var abilityEntry = context.Entry(ability);
            if (abilityEntry.State != EntityState.Detached)
            {
                return;
            }

            abilityEntry.State = EntityState.Unchanged;

            foreach (var effect in ability.Effects)
            {
                Attach(effect, context);
            }

            foreach (var trigger in ability.Triggers)
            {
                Attach(trigger, context);
            }
        }

        public static void Attach(AppliedEffect effect, GameDbContext context)
        {
            var effectEntry = context.Entry(effect);
            if (effectEntry.State != EntityState.Detached)
            {
                return;
            }

            effectEntry.State = EntityState.Unchanged;

            Attach(effect.SourceAbility, context);

            if (effect is MeleeAttacked meleeAttacked)
            {
                Attach(meleeAttacked.Weapon, context);
            }
            else if (effect is RangeAttacked rangeAttacked)
            {
                Attach(rangeAttacked.Weapon, context);
            }
        }

        public static void Attach(Effect effect, GameDbContext context)
        {
            var effectEntry = context.Entry(effect);
            if (effectEntry.State != EntityState.Detached)
            {
                return;
            }

            effectEntry.State = EntityState.Unchanged;

            if (effect is AddAbility addAbility)
            {
                Attach(addAbility.Ability, context);
            }
        }

        public static void Attach(Trigger trigger, GameDbContext context)
        {
            var triggerEntry = context.Entry(trigger);
            if (triggerEntry.State != EntityState.Detached)
            {
                return;
            }

            triggerEntry.State = EntityState.Unchanged;
        }

        public static void Attach(Item item, GameDbContext context)
        {
            var itemEntry = context.Entry(item);
            if (itemEntry.State != EntityState.Detached)
            {
                return;
            }

            itemEntry.State = EntityState.Unchanged;

            foreach (var ability in item.Abilities)
            {
                Attach(ability, context);
            }

            foreach (var effect in item.ActiveEffects)
            {
                Attach(effect, context);
            }

            foreach (Property property in item.Properties)
            {
                Attach(property, context);
            }

            if (item is Launcher launcher)
            {
                Attach(launcher.Projectile, context);
            }
            else if (item is Container container)
            {
                foreach (var containedItem in container.Items)
                {
                    Attach(containedItem, context);
                }
            }
        }
    }
}