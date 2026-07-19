using UnicornHack.Systems.Abilities;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Effects;

namespace UnicornHack.Utils.MessagingECS;

public class SortedUniqueEntityIndexTest
{
    [Fact]
    public void Index_is_updated_when_a_key_component_is_added_without_entering_the_group()
    {
        var manager = TestHelper.CreateGameManager();

        using var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        var activeEffects = entity.AddComponent<ActiveEffectsComponent>((int)EntityComponent.ActiveEffects);
        activeEffects.NextEffectTick = 20;
        Assert.Same(entity, manager.TemporalEntitiesIndex[(20, entity.Id)]);

        var player = entity.AddComponent<PlayerComponent>((int)EntityComponent.Player);
        player.NextActionTick = 10;

        Assert.Equal(1, manager.TemporalEntitiesIndex.Count);
        Assert.Null(manager.TemporalEntitiesIndex[(20, entity.Id)]);
        Assert.Same(entity, manager.TemporalEntitiesIndex[(10, entity.Id)]);
    }

    [Fact]
    public void Index_is_updated_when_a_key_component_is_removed_without_leaving_the_group()
    {
        var manager = TestHelper.CreateGameManager();

        using var entityReference = manager.CreateEntity();
        var entity = entityReference.Referenced;
        var ai = entity.AddComponent<AIComponent>((int)EntityComponent.AI);
        var activeEffects = entity.AddComponent<ActiveEffectsComponent>((int)EntityComponent.ActiveEffects);

        ai.NextActionTick = 10;
        activeEffects.NextEffectTick = 20;
        ai.NextActionTick = null;

        Assert.Same(entity, manager.TemporalEntitiesIndex[(20, entity.Id)]);

        entity.RemoveComponent(activeEffects);

        Assert.Equal(0, manager.TemporalEntitiesIndex.Count);
    }

    [Fact]
    public void Index_is_updated()
    {
        var manager = TestHelper.CreateGameManager();

        Assert.Equal(0, manager.TemporalEntitiesIndex.Count);

        using (var firstAbilityEntityReference = manager.CreateEntity())
        {
            var firstAbilityEntity = firstAbilityEntityReference.Referenced;
            var firstAbility = firstAbilityEntity.AddComponent<AbilityComponent>((int)EntityComponent.Ability);

            firstAbility.CooldownTick = 10;

            Assert.Equal(1, manager.TemporalEntitiesIndex.Count);
            Assert.Same(firstAbilityEntity, manager.TemporalEntitiesIndex[(10, firstAbilityEntity.Id)]);

            using (var secondAbilityEntityReference = manager.CreateEntity())
            {
                var secondAbilityEntity = secondAbilityEntityReference.Referenced;
                var secondAbility = secondAbilityEntity.AddComponent<AbilityComponent>((int)EntityComponent.Ability);

                secondAbility.CooldownTick = 9;

                Assert.Same(secondAbilityEntity, manager.TemporalEntitiesIndex.First());
                Assert.Same(firstAbilityEntity, manager.TemporalEntitiesIndex.Last());
                Assert.Equal(2, manager.TemporalEntitiesIndex.Count);

                secondAbility.CooldownTick = 11;

                Assert.Same(firstAbilityEntity, manager.TemporalEntitiesIndex.First());
                Assert.Same(secondAbilityEntity, manager.TemporalEntitiesIndex.Last());

                secondAbility.CooldownTick = null;

                Assert.Same(firstAbilityEntity, manager.TemporalEntitiesIndex.First());
                Assert.Same(firstAbilityEntity, manager.TemporalEntitiesIndex.Last());
            }

            manager.Queue.ProcessQueue(manager);
        }

        Assert.Equal(0, manager.TemporalEntitiesIndex.Count);
    }
}
