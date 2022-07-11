using UnicornHack.Systems.Items;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities;

public class AbilitySlottingSystem :
    IGameSystem<SetAbilitySlotMessage>,
    IGameSystem<EntityAddedMessage<GameEntity>>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, bool>>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, int>>
{
    public static readonly int DefaultMeleeAttackSlot = 0;
    public static readonly int DefaultRangedAttackSlot = 1;
    public static readonly int DefaultSlotCapacity = 8;

    public MessageProcessingResult Process(SetAbilitySlotMessage message, GameManager manager)
    {
        var ability = message.AbilityEntity?.Ability
                      ?? manager.AffectableAbilitiesIndex[(message.OwnerEntity.Id, message.AbilityName)]?.Ability;

        if (message.Slot != null)
        {
            var conflictingAbility = (ability?.OwnerEntity ?? message.OwnerEntity).Being!.SlottedAbilities
                .GetValueOrDefault(message.Slot.Value);
            if (conflictingAbility != null)
            {
                if (conflictingAbility == message.AbilityEntity)
                {
                    return MessageProcessingResult.ContinueProcessing;
                }

                ResetSlot(conflictingAbility, manager);

                if (conflictingAbility.Ability!.Slot != null)
                {
                    return MessageProcessingResult.ContinueProcessing;
                }
            }

            if (ability == null)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            Debug.Assert(ability.CooldownTick == null && ability.CooldownXpLeft == null);

            if (message.Slot.Value < 0
                || message.Slot.Value >= ability.OwnerEntity!.Physical!.Capacity)
            {
                throw new InvalidOperationException("Invalid slot " + message.Slot.Value);
            }

            if ((ability.Activation & ActivationType.Slottable) == 0)
            {
                throw new InvalidOperationException($"Ability {ability.EntityId} is not slottable.");
            }

            if (!ability.IsUsable)
            {
                throw new InvalidOperationException($"Ability {ability.EntityId} is not usable.");
            }

            if ((message.Slot.Value == DefaultMeleeAttackSlot
                 || message.Slot.Value == DefaultRangedAttackSlot)
                && ability.Type != AbilityType.DefaultAttack)
            {
                throw new InvalidOperationException(
                    "Ability " + ability.EntityId + " cannot be the default attack for " + ability.OwnerId);
            }

            if (message.Slot.Value != DefaultMeleeAttackSlot
                && message.Slot.Value != DefaultRangedAttackSlot
                && ability.Type == AbilityType.DefaultAttack)
            {
                throw new InvalidOperationException(
                    "Ability " + ability.EntityId + " can only be the default attack for " + ability.OwnerId);
            }

            ability.Slot = message.Slot;
        }
        else if (ability != null)
        {
            if (ability.IsActive
                && ability.Slot != null)
            {
                var deactivateMessage =
                    DeactivateAbilityMessage.Create(manager);
                deactivateMessage.AbilityEntity = ability.Entity;
                deactivateMessage.ActivatorEntity = ability.OwnerEntity!;

                manager.Process(deactivateMessage);
            }

            if (ability.IsActive
                || ability.CooldownTick != null
                || ability.CooldownXpLeft != null)
            {
                return MessageProcessingResult.StopProcessing;
            }

            ability.Slot = null;

            if (ability.Type == AbilityType.Item)
            {
                var itemId = GetTargetItemId(ability.Entity, manager);

                if (manager.FindEntity(itemId)!.Item!.EquippedSlot == EquipmentSlot.None)
                {
                    var position = ability.OwnerEntity!.Position!;
                    var dropMessage = MoveItemMessage.Create(manager);
                    dropMessage.ItemEntity = manager.FindEntity(itemId)!;
                    dropMessage.TargetLevelEntity = position.LevelEntity;
                    dropMessage.TargetCell = position.LevelCell;

                    manager.Enqueue(dropMessage);
                }
            }
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
    {
        // AbilitiesToAffectableRelationship
        if (message.PrincipalEntity == null
            || !message.PrincipalEntity.HasComponent(EntityComponent.Being))
        {
            return MessageProcessingResult.ContinueProcessing;
        }

        var ability = message.Entity.Ability!;
        if ((ability.Activation & ActivationType.Slottable) != 0
            && ability.Type == AbilityType.Item)
        {
            Debug.Assert(!ability.IsActive);

            var slot = GetFirstFreeSlot(message.PrincipalEntity);
            if (slot != null)
            {
                var setSlotMessage = SetAbilitySlotMessage.Create(manager);
                setSlotMessage.AbilityEntity = message.Entity;
                setSlotMessage.Slot = slot;

                Process(setSlotMessage, manager);

                manager.Queue.ReturnMessage(setSlotMessage);
                return MessageProcessingResult.ContinueProcessing;
            }

            var itemId = GetTargetItemId(message.Entity, manager);

            var position = message.PrincipalEntity.Position!;
            var dropMessage = MoveItemMessage.Create(manager);
            dropMessage.ItemEntity = manager.FindEntity(itemId)!;
            dropMessage.TargetLevelEntity = position.LevelEntity;
            dropMessage.TargetCell = position.LevelCell;
            dropMessage.SuppressLog = true;

            manager.Enqueue(dropMessage);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private static int GetTargetItemId(GameEntity abilityEntity, GameManager manager)
    {
        var itemId = 0;
        foreach (var effectEntity in abilityEntity.Ability!.Effects)
        {
            var effect = effectEntity.Effect!;
            switch (effect.EffectType)
            {
                case EffectType.Activate:
                    itemId = manager.FindEntity(effect.TargetEntityId)!.Ability!.OwnerId!.Value;
                    break;
                case EffectType.EquipItem:
                case EffectType.Move:
                    itemId = effect.TargetEntityId!.Value;
                    break;
                default:
                    continue;
            }
        }

        return itemId;
    }

    public MessageProcessingResult Process(
        PropertyValueChangedMessage<GameEntity, bool> message, GameManager manager)
    {
        var ability = message.Entity.Ability!;
        Debug.Assert(ability.CooldownXpLeft == null && ability.CooldownTick == null);
        if (!ability.IsUsable
            && ability.Slot != null)
        {
            ResetSlot(message.Entity, manager);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(
        PropertyValueChangedMessage<GameEntity, int> message, GameManager manager)
    {
        // Physical.Capacity
        if (message.NewValue < message.OldValue)
        {
            foreach (var abilityEntity in message.Entity.Physical!.Abilities)
            {
                var abilitySlot = abilityEntity.Ability!.Slot;
                if (abilitySlot.HasValue
                    && abilitySlot != DefaultMeleeAttackSlot
                    && abilitySlot != DefaultRangedAttackSlot
                    && abilitySlot >= message.NewValue)
                {
                    ResetSlot(abilityEntity, manager);
                }
            }
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    private void ResetSlot(GameEntity abilityEntity, GameManager manager)
    {
        var resetMessage = SetAbilitySlotMessage.Create(manager);
        resetMessage.AbilityEntity = abilityEntity;

        Process(resetMessage, manager);

        manager.Queue.ReturnMessage(resetMessage);
    }

    public GameEntity? GetAbility(GameEntity ownerEntity, int slot)
        => ownerEntity.Being!.SlottedAbilities.GetValueOrDefault(slot);

    private readonly int FirstPotentialSlot = Math.Max(DefaultMeleeAttackSlot, DefaultRangedAttackSlot) + 1;

    public int? GetFirstFreeSlot(GameEntity owner)
    {
        var abilities = owner.Being!.SlottedAbilities;
        var i = FirstPotentialSlot;
        for (; i < owner.Physical!.Capacity; i++)
        {
            if (!abilities.ContainsKey(i))
            {
                break;
            }
        }

        return i < owner.Physical.Capacity ? i : null;
    }
}
