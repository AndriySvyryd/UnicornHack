using UnicornHack.Systems.Knowledge;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Systems.Beings;

public class LivingSystem :
    IGameSystem<XPGainedMessage>,
    IGameSystem<PropertyValueChangedMessage<GameEntity, int>>,
    IGameSystem<EntityAddedMessage<GameEntity>>,
    IEntityChangeListener<GameEntity>
{
    /// <summary>
    ///     The evasion every being has before its speed-derived agility bonus; a flat-footed target is
    ///     reduced to this value.
    /// </summary>
    public const int BaseEvasion = 50;

    public MessageProcessingResult Process(XPGainedMessage message, GameManager manager)
    {
        var being = message.Entity.Being!;

        if (being.Regeneration > 0)
        {
            being.HitPoints += being.Regeneration * message.ExperiencePoints;
        }

        if (being.EnergyRegeneration > 0)
        {
            var restoration = (long)being.EnergyRegeneration * message.ExperiencePoints;
            being.EnergyPoints += (int)((restoration + 2) / 3);
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(
        PropertyValueChangedMessage<GameEntity, int> message, GameManager manager)
    {
        var being = message.Entity.Being!;
        switch (message.ChangedPropertyName)
        {
            case nameof(BeingComponent.HitPoints):
                if (message.OldValue > 0
                    && message.NewValue <= 0)
                {
                    being.HitPoints = 0;
                    DiedMessage.Enqueue(message.Entity, manager);
                    if (!message.Entity.HasComponent(EntityComponent.Player))
                    {
                        DeleteEntityMessage.Enqueue(message.Entity, manager);
                    }
                }

                break;
        }

        return MessageProcessingResult.ContinueProcessing;
    }

    public MessageProcessingResult Process(EntityAddedMessage<GameEntity> message, GameManager manager)
    {
        var being = message.Entity.Being!;

        // Spawn at full health so the being counts as alive while the rest of its body and stat setup
        // runs; several handlers (natural-weapon recalculation, etc.) skip a being that reads as dead.
        being.HitPoints = being.HitPointMaximum;
        being.EnergyPoints = being.EnergyPointMaximum;
        return MessageProcessingResult.ContinueProcessing;
    }

    void IEntityChangeListener<GameEntity>.OnEntityAdded(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnEntityRemoved(in EntityChange<GameEntity> entityChange)
    {
    }

    void IEntityChangeListener<GameEntity>.OnPropertyValuesChanged(in EntityChange<GameEntity> entityChange)
    {
        var changes = entityChange.PropertyChanges;
        for (var index = 0; index < changes.Count; index++)
        {
            if (changes.GetChangedComponent(index) is not BeingComponent being)
            {
                continue;
            }

            var property = changes.GetChangedPropertyName(index);
            switch (property)
            {
                case nameof(BeingComponent.HitPointMaximum):
                case nameof(BeingComponent.EnergyPointMaximum):
                    var oldMaximum = changes.GetValue<int>(index, Utils.MessagingECS.ValueType.Old);
                    var newMaximum = changes.GetValue<int>(index, Utils.MessagingECS.ValueType.Current);
                    if (property == nameof(BeingComponent.HitPointMaximum))
                    {
                        being.HitPoints = oldMaximum == 0 ? newMaximum : being.HitPoints * newMaximum / oldMaximum;
                    }
                    else
                    {
                        being.EnergyPoints = oldMaximum == 0 ? newMaximum : being.EnergyPoints * newMaximum / oldMaximum;
                    }

                    break;
                case nameof(BeingComponent.ReservedHitPoints):
                    being.HitPoints = being.HitPoints;
                    break;
                case nameof(BeingComponent.ReservedEnergyPoints):
                    being.EnergyPoints = being.EnergyPoints;
                    break;
            }
        }
    }
}
