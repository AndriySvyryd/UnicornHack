using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Utils.MessagingECS;

// ReSharper disable once CheckNamespace
namespace UnicornHack
{
    public partial class GameManager
    {
        public EntityGroup<GameEntity> Abilities { get; private set; }
        public EntityGroup<GameEntity> AffectableEntities { get; private set; }
        public EntityRelationship<GameEntity> AbilitiesToAffectableRelationship { get; private set; }
        public AbilityActivationSystem AbilityActivationSystem { get; private set; }

        private void InitializeAbilities(SequentialMessageQueue<GameManager> queue)
        {
            Add<AbilityComponent>(EntityComponent.Ability, poolSize: 32);

            AffectableEntities = CreateGroup(nameof(AffectableEntities),
                new EntityMatcher<GameEntity>().AnyOf(
                    (int)EntityComponent.Being, (int)EntityComponent.Physical, (int)EntityComponent.Item));

            Abilities = CreateGroup(nameof(Abilities),
                new EntityMatcher<GameEntity>().AllOf((int)EntityComponent.Ability));

            AbilitiesToAffectableRelationship = new EntityRelationship<GameEntity>(
                nameof(AbilitiesToAffectableRelationship),
                Abilities,
                AffectableEntities,
                new SimpleNonNullableKeyValueGetter<GameEntity, int>(
                    component => ((AbilityComponent)component).OwnerId,
                    (int)EntityComponent.Ability),
                (effectEntity, _, __, ___) => effectEntity.RemoveComponent((int)EntityComponent.Ability),
                referencedKeepAlive: false,
                referencingKeepAlive: true);

            AbilityActivationSystem = new AbilityActivationSystem();
            queue.Add<ActivateAbilityMessage>(AbilityActivationSystem,
                AbilityActivationSystem.ActivateAbilityMessageName, 0);
            queue.Add<ItemMovedMessage>(AbilityActivationSystem, ItemMovingSystem.ItemMovedMessageName, 3);
            queue.Add<ItemEquippedMessage>(AbilityActivationSystem, ItemUsageSystem.ItemEquippedMessageName, 3);
            queue.Add<DiedMessage>(AbilityActivationSystem, LivingSystem.DiedMessageName, 3);
            queue.Add<EntityAddedMessage<GameEntity>>(
                AbilityActivationSystem, AbilitiesToAffectableRelationship.GetEntityAddedMessageName(), 0);
            queue.Add<EntityRemovedMessage<GameEntity>>(
                AbilityActivationSystem, AbilitiesToAffectableRelationship.GetEntityRemovedMessageName(), 0);
        }
    }
}
