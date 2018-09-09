﻿using System.Collections.Immutable;
using UnicornHack.Primitives;
using UnicornHack.Utils.DataStructures;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Abilities
{
    public class AbilityActivatedMessage : IMessage
    {
        private GameEntity _activatorEntity;
        private GameEntity _abilityEntity;
        private GameEntity _targetEntity;

        public GameEntity ActivatorEntity
        {
            get => _activatorEntity;
            set
            {
                _activatorEntity?.RemoveReference(this);
                _activatorEntity = value;
                _activatorEntity?.AddReference(this);
            }
        }

        public GameEntity AbilityEntity
        {
            get => _abilityEntity;
            set
            {
                _abilityEntity?.RemoveReference(this);
                _abilityEntity = value;
                _abilityEntity?.AddReference(this);
            }
        }

        public GameEntity TargetEntity
        {
            get => _targetEntity;
            set
            {
                _targetEntity?.RemoveReference(this);
                _targetEntity = value;
                _targetEntity?.AddReference(this);
            }
        }

        public Point? TargetCell { get; set; }
        public ActivationType Trigger { get; set; }
        public ImmutableList<GameEntity> EffectsToApply { get; set; }
        public bool SuccessfulActivation { get; set; }
        public bool SuccessfulApplication { get; set; }
        public int Delay { get; set; }

        public AbilityActivatedMessage Clone(GameManager manager)
        {
            var abilityActivatedMessage = manager.Queue.CreateMessage<AbilityActivatedMessage>(
                ((IMessage)this).MessageName);
            abilityActivatedMessage.ActivatorEntity = ActivatorEntity;
            abilityActivatedMessage.AbilityEntity = AbilityEntity;
            abilityActivatedMessage.TargetEntity = TargetEntity;
            abilityActivatedMessage.TargetCell = TargetCell;
            abilityActivatedMessage.Trigger = Trigger;
            abilityActivatedMessage.EffectsToApply = EffectsToApply;
            abilityActivatedMessage.SuccessfulActivation = SuccessfulActivation;
            abilityActivatedMessage.Delay = Delay;
            return abilityActivatedMessage;
        }

        string IMessage.MessageName { get; set; }

        public void Dispose()
        {
            ActivatorEntity = null;
            AbilityEntity = null;
            TargetEntity = null;
        }
    }
}