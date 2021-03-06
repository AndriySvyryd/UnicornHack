﻿using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Beings
{
    public class DelayMessage : IMessage
    {
        public const string Name = "Delay";

        public static void Enqueue(GameEntity actorEntity, int delay, GameManager manager)
        {
            var delayMessage = manager.Queue.CreateMessage<DelayMessage>(Name);
            delayMessage.ActorEntity = actorEntity;
            delayMessage.Delay = delay;
            manager.Enqueue(delayMessage);
        }

        private GameEntity _actorEntity;

        public GameEntity ActorEntity
        {
            get => _actorEntity;
            set
            {
                _actorEntity?.RemoveReference(this);
                _actorEntity = value;
                _actorEntity?.AddReference(this);
            }
        }

        public int Delay { get; set; }

        string IMessage.MessageName { get; set; }

        public void Clean()
        {
            ActorEntity = default;
            Delay = default;
        }
    }
}
