using System;
using System.Diagnostics;
using UnicornHack.Primitives;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities
{
    public class AbilitySlottingSystem :
        IGameSystem<SetAbilitySlotMessage>,
        IGameSystem<PropertyValueChangedMessage<GameEntity, bool>>,
        IGameSystem<PropertyValueChangedMessage<GameEntity, int>>
    {
        public static int DefaultMeleeAttackSlot = -1;
        public static int DefaultRangedAttackSlot = -2;
        public const string SetAbilitySlotMessageName = "SetAbilitySlot";

        public SetAbilitySlotMessage CreateSetAbilitySlotMessage(GameManager manager)
            => manager.Queue.CreateMessage<SetAbilitySlotMessage>(SetAbilitySlotMessageName);

        public MessageProcessingResult Process(SetAbilitySlotMessage message, GameManager manager)
        {
            var ability = message.AbilityEntity?.Ability
                ?? manager.AffectableAbilitiesIndex[(message.OwnerEntity.Id, message.AbilityName)]?.Ability;

            if (message.Slot != null)
            {
                var conflictingAbility =
                    manager.SlottedAbilitiesIndex[(ability?.OwnerId ?? message.OwnerEntity.Id, message.Slot.Value)];
                if (conflictingAbility != null)
                {
                    if (conflictingAbility == message.AbilityEntity)
                    {
                        return MessageProcessingResult.ContinueProcessing;
                    }

                    ResetSlot(conflictingAbility, manager);

                    if (conflictingAbility.Ability.Slot != null)
                    {
                        return MessageProcessingResult.ContinueProcessing;
                    }
                }

                if (ability == null)
                {
                    return MessageProcessingResult.ContinueProcessing;
                }

                Debug.Assert(ability.CooldownTick == null && ability.CooldownXpLeft == null);

                if (message.Slot.Value != DefaultMeleeAttackSlot
                    && message.Slot.Value != DefaultRangedAttackSlot
                    && (message.Slot.Value < 0
                        || message.Slot.Value >= ability.OwnerEntity.Being.AbilitySlotCount))
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
                    && ability.Template?.Type != AbilityType.DefaultAttack)
                {
                    throw new InvalidOperationException(
                        "Ability " + ability.EntityId + " cannot be the default attack for " + ability.OwnerId);
                }

                if (message.Slot.Value != DefaultMeleeAttackSlot
                    && message.Slot.Value != DefaultRangedAttackSlot
                    && ability.Template?.Type == AbilityType.DefaultAttack)
                {
                    throw new InvalidOperationException(
                        "Ability " + ability.EntityId + " can only be the default attack for " + ability.OwnerId);
                }

                var oldSlot = ability.Slot;
                ability.Slot = message.Slot;

                if (oldSlot == null
                    && (ability.Activation & ActivationType.WhileToggled) != 0)
                {
                    var activationMessage = manager.AbilityActivationSystem.CreateActivateAbilityMessage(manager);
                    activationMessage.AbilityEntity = message.AbilityEntity;
                    activationMessage.ActivatorEntity = ability.OwnerEntity;
                    activationMessage.TargetEntity = ability.OwnerEntity;

                    if (!manager.AbilityActivationSystem.CanActivateAbility(activationMessage, shouldThrow: false))
                    {
                        manager.Queue.ReturnMessage(activationMessage);
                        ability.Slot = oldSlot;
                        return MessageProcessingResult.ContinueProcessing;
                    }

                    manager.Enqueue(activationMessage);
                }
            }
            else if (ability != null)
            {
                if (ability.IsActive
                    && ability.Slot != null)
                {
                    var deactivateMessage =
                        manager.AbilityActivationSystem.CreateDeactivateAbilityMessage(manager);
                    deactivateMessage.AbilityEntity = message.AbilityEntity;
                    deactivateMessage.ActivatorEntity = ability.OwnerEntity;

                    manager.Process(deactivateMessage);
                }

                if (ability.CooldownTick == null
                    && ability.CooldownXpLeft == null)
                {
                    ability.Slot = null;
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(
            PropertyValueChangedMessage<GameEntity, bool> message, GameManager manager)
        {
            var ability = message.Entity.Ability;
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
            if (message.NewValue < message.OldValue)
            {
                foreach (var abilityEntity in manager.AbilitiesToAffectableRelationship[message.Entity.Id])
                {
                    var abilitySlot = abilityEntity.Ability.Slot;
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
            var resetMessage = CreateSetAbilitySlotMessage(manager);
            resetMessage.AbilityEntity = abilityEntity;

            Process(resetMessage, manager);

            manager.Queue.ReturnMessage(resetMessage);
        }

        public GameEntity GetAbility(int ownerId, int slot, GameManager manager)
        {
            foreach (var abilityEntity in manager.AbilitiesToAffectableRelationship[ownerId])
            {
                if (abilityEntity.Ability.Slot == slot)
                {
                    return abilityEntity;
                }
            }

            return null;
        }
    }
}
