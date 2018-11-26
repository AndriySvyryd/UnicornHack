using System;
using System.Diagnostics;
using UnicornHack.Primitives;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities
{
    public class AbilitySlottingSystem:
        IGameSystem<SetAbilitySlotMessage>,
        IGameSystem<PropertyValueChangedMessage<GameEntity, bool>>,
        IGameSystem<PropertyValueChangedMessage<GameEntity, int>>
    {
        public static int DefaultAttackSlot = -1;
        public const string SetAbilitySlotMessageName = "SetAbilitySlot";

        public SetAbilitySlotMessage CreateSetAbilitySlotMessage(GameManager manager)
            => manager.Queue.CreateMessage<SetAbilitySlotMessage>(SetAbilitySlotMessageName);

        public MessageProcessingResult Process(SetAbilitySlotMessage message, GameManager manager)
        {
            var ability = message.AbilityEntity.Ability;
            if ((ability.Activation & ActivationType.Slottable) == 0)
            {
                return MessageProcessingResult.ContinueProcessing;
            }

            if (message.Slot == null)
            {
                if (ability.IsActive)
                {
                    manager.AbilityActivationSystem.Deactivate(ability, manager);
                }

                ability.Slot = null;
            }
            else
            {
                if (message.Slot.Value != DefaultAttackSlot
                    && (message.Slot.Value < 0
                        || message.Slot.Value >= ability.OwnerEntity.Being.AbilitySlotCount))
                {
                    throw new InvalidOperationException("Invalid slot " + message.Slot.Value);
                }

                if (!ability.IsUsable)
                {
                    return MessageProcessingResult.ContinueProcessing;
                }

                if (message.Slot.Value == DefaultAttackSlot
                    && !manager.SkillAbilitiesSystem.CanBeDefaultAttack(ability))
                {
                    throw new InvalidOperationException(
                        "Ability " + ability.EntityId + " cannot be the default attack for " + ability.OwnerId);
                }

                if (message.Slot.Value != DefaultAttackSlot
                    && manager.SkillAbilitiesSystem.CanBeDefaultAttack(ability))
                {
                    throw new InvalidOperationException(
                        "Ability " + ability.EntityId + " can only be the default attack for " + ability.OwnerId);
                }

                var conflictingAbility = manager.SlottedAbilitiesIndex[(ability.OwnerId.Value, message.Slot.Value)];
                if (conflictingAbility != null)
                {
                    ResetSlot(conflictingAbility, manager);

                    Debug.Assert(conflictingAbility.Ability.Slot == null);
                }

                var oldSlot = ability.Slot;
                ability.Slot = message.Slot;

                if ((ability.Activation & ActivationType.WhileToggled) != 0
                    && oldSlot == null)
                {
                    var activationMessage = manager.AbilityActivationSystem.CreateActivateAbilityMessage(manager);
                    activationMessage.AbilityEntity = message.AbilityEntity;
                    activationMessage.ActivatorEntity = ability.OwnerEntity;
                    activationMessage.TargetEntity = ability.OwnerEntity;

                    if (!manager.AbilityActivationSystem.CanActivateAbility(activationMessage))
                    {
                        manager.Queue.ReturnMessage(activationMessage);
                        return MessageProcessingResult.ContinueProcessing;
                    }

                    manager.Enqueue(activationMessage);
                }
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(PropertyValueChangedMessage<GameEntity, bool> message, GameManager manager)
        {
            var ability = message.Entity.Ability;
            if (!ability.IsUsable
                && ability.Slot != null)
            {
                ResetSlot(message.Entity, manager);
            }

            return MessageProcessingResult.ContinueProcessing;
        }

        public MessageProcessingResult Process(PropertyValueChangedMessage<GameEntity, int> message, GameManager manager)
        {
            if (message.NewValue < message.OldValue)
            {
                foreach (var abilityEntity in manager.AbilitiesToAffectableRelationship[message.Entity.Id])
                {
                    var abilitySlot = abilityEntity.Ability.Slot;
                    if (abilitySlot.HasValue
                        && abilitySlot != DefaultAttackSlot
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
