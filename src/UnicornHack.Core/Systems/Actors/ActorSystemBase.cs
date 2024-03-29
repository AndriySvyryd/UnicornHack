﻿using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Systems.Actors;

public abstract class ActorSystemBase
{
    protected bool ActivateAbility(GameEntity actorEntity, int? slot, int? targetId, GameManager manager)
    {
        Point targetCell;
        GameEntity? targetActor;
        if (targetId == null)
        {
            targetCell = actorEntity.Position!.LevelCell;
            targetActor = actorEntity;
        }
        else if (targetId.Value < 0)
        {
            targetActor = manager.FindEntity(-targetId.Value);
            if (targetActor == null
                || !targetActor.Being!.IsAlive)
            {
                return false;
            }

            targetCell = targetActor.Position!.LevelCell;
        }
        else
        {
            targetCell = Point.Unpack(targetId)!.Value;
            targetActor = actorEntity.Position!.LevelEntity.Level!.Actors.GetValueOrDefault(targetCell);
        }

        GameEntity? abilityEntity;
        if (slot.HasValue)
        {
            abilityEntity = manager.AbilitySlottingSystem.GetAbility(actorEntity, slot.Value);
            if (abilityEntity == null)
            {
                throw new InvalidOperationException("Actor " + actorEntity.Id + ". No ability in slot " + targetId);
            }
        }
        else
        {
            var vectorToTarget = actorEntity.Position!.LevelCell.DifferenceTo(targetCell);
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

    public virtual GameEntity? GetDefaultAttack(GameEntity actorEntity, bool melee, GameManager manager)
        => manager.AbilitySlottingSystem.GetAbility(
            actorEntity,
            melee
                ? AbilitySlottingSystem.DefaultMeleeAttackSlot
                : AbilitySlottingSystem.DefaultRangedAttackSlot);

    protected virtual bool ActivateAbility(
        GameEntity abilityEntity, GameEntity actorEntity, Point targetCell, GameEntity? targetEntity,
        GameManager manager)
    {
        var ability = abilityEntity.Ability!;
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
        var position = actorEntity.Position!;
        var targetCell = onlyChangeHeading
            ? position.LevelCell
            : position.LevelCell.Translate(direction.AsVector());
        if (!onlyChangeHeading
            && targetCell != position.LevelCell)
        {
            var conflictingActor = position.LevelEntity.Level!.Actors.GetValueOrDefault(targetCell);
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
                    actorEntity, AbilitySlottingSystem.DefaultMeleeAttackSlot);
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
            || itemEntity.Item!.ContainerId != actorEntity.Id)
        {
            throw new InvalidOperationException("Actor " + actorEntity.Id + " doesn't have item " + itemId);
        }

        return itemEntity;
    }
}
