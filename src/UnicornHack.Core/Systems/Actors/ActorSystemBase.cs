using System;
using UnicornHack.Primitives;
using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;

namespace UnicornHack.Systems.Actors
{
    public abstract class ActorSystemBase
    {
        protected bool ActivateAbility(GameEntity actorEntity, int? slot, int? target, GameManager manager)
        {
            Point targetCell;
            GameEntity targetActor;
            if (target == null)
            {
                targetCell = actorEntity.Position.LevelCell;
                targetActor = actorEntity;
            }
            else if (target.Value < 0)
            {
                targetActor = manager.FindEntity(-target.Value);
                if (targetActor == null
                    || !targetActor.Being.IsAlive)
                {
                    return false;
                }

                targetCell = targetActor.Position.LevelCell;
            }
            else
            {
                targetCell = Point.Unpack(target).Value;
                targetActor =
                    manager.LevelActorToLevelCellIndex[(actorEntity.Position.LevelId, targetCell.X, targetCell.Y)];
            }

            GameEntity abilityEntity;
            if (slot.HasValue)
            {
                abilityEntity = manager.AbilitySlottingSystem.GetAbility(actorEntity.Id, slot.Value, manager);
                if (abilityEntity == null)
                {
                    throw new InvalidOperationException("Actor " + actorEntity.Id + ". No ability in slot " + target);
                }
            }
            else
            {
                var vectorToTarget = actorEntity.Position.LevelCell.DifferenceTo(targetCell);
                var isMelee = vectorToTarget.Length() <= 1;
                abilityEntity = GetDefaultAttack(actorEntity, isMelee, manager);
                if (abilityEntity == null)
                {
                    if (actorEntity.HasComponent(EntityComponent.Player))
                    {
                        manager.LoggingSystem.WriteLog(
                            manager.Game.Services.Language.NoDefaultAttack(isMelee), actorEntity, manager);
                    }
                    else
                    {
                        throw new InvalidOperationException("Actor " + actorEntity.Id + ". No usable default attack.");
                    }
                    return false;
                }
            }

            return ActivateAbility(abilityEntity, actorEntity, targetCell, targetActor, manager);
        }

        public virtual GameEntity GetDefaultAttack(GameEntity actorEntity, bool melee, GameManager manager)
            => manager.AbilitySlottingSystem.GetAbility(
                actorEntity.Id,
                melee
                    ? AbilitySlottingSystem.DefaultMeleeAttackSlot
                    : AbilitySlottingSystem.DefaultRangedAttackSlot,
                manager);

        protected virtual bool ActivateAbility(
            GameEntity abilityEntity, GameEntity actorEntity, Point targetCell, GameEntity targetEntity, GameManager manager)
        {
            var ability = abilityEntity.Ability;
            if ((ability.Activation & ActivationType.Slottable) == 0)
            {
                throw new InvalidOperationException("Ability " + abilityEntity.Id + " cannot be activated directly.");
            }

            var activationMessage = ActivateAbilityMessage.Create(manager);
            activationMessage.AbilityEntity = abilityEntity;
            activationMessage.ActivatorEntity = actorEntity;
            activationMessage.TargetEntity = targetEntity;
            activationMessage.TargetCell = targetCell;

            if (!manager.AbilityActivationSystem.CanActivateAbility(
                activationMessage, shouldThrow: actorEntity.HasComponent(EntityComponent.Player)))
            {
                manager.Queue.ReturnMessage(activationMessage);
                return false;
            }

            manager.Enqueue(activationMessage);
            return true;
        }

        protected bool Move(
            Direction direction, GameEntity actorEntity, GameManager manager, bool onlyChangeHeading = false)
        {
            // TODO: only attack on move if hostile
            var position = actorEntity.Position;
            var targetCell = onlyChangeHeading
                ? position.LevelCell
                : position.LevelCell.Translate(direction.AsVector());
            if (!onlyChangeHeading
                && targetCell != position.LevelCell)
            {
                var conflictingActor = manager.LevelActorToLevelCellIndex[(position.LevelId, targetCell.X, targetCell.Y)];
                if (conflictingActor != null)
                {
                    if (position.Heading != direction)
                    {
                        var turnMessage = TravelMessage.Create(manager);
                        turnMessage.ActorEntity = actorEntity;
                        turnMessage.TargetHeading = direction;
                        turnMessage.TargetCell = position.LevelCell;

                        manager.Enqueue(turnMessage);
                        return true;
                    }

                    var abilityEntity = manager.AbilitySlottingSystem.GetAbility(
                        actorEntity.Id, AbilitySlottingSystem.DefaultMeleeAttackSlot, manager);
                    return abilityEntity != null
                           && ActivateAbility(abilityEntity, actorEntity, targetCell, conflictingActor, manager);
                }
            }

            var travelMessage = TravelMessage.Create(manager);
            travelMessage.ActorEntity = actorEntity;
            travelMessage.TargetHeading = direction;
            travelMessage.TargetCell = targetCell;
            travelMessage.MoveOffConflicting = actorEntity.HasComponent(EntityComponent.Player);

            manager.Enqueue(travelMessage);
            return true;
        }

        protected GameEntity GetItem(int itemId, GameEntity actorEntity, GameManager manager)
        {
            var itemEntity = manager.FindEntity(itemId);
            if (itemEntity == null
                || itemEntity.Item.ContainerId != actorEntity.Id)
            {
                throw new InvalidOperationException("Actor " + actorEntity.Id + ". Invalid item " + itemId);
            }

            return itemEntity;
        }
    }
}
