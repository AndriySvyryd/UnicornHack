using UnicornHack.Systems.Effects;

namespace UnicornHack.Utils.MessagingECS;

public class SortedUniqueEntityIndexTest
{
    [Fact]
    public void Index_is_updated()
    {
        var manager = TestHelper.CreateGameManager();

        Assert.Equal(0, manager.TemporalEntitiesIndex.Count);

        using (var firstEffectEntityReference = manager.CreateEntity())
        {
            var firstEffectEntity = firstEffectEntityReference.Referenced;
            var firstEffect = firstEffectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);

            firstEffect.ExpirationTick = 10;

            Assert.Equal(1, manager.TemporalEntitiesIndex.Count);
            Assert.Same(firstEffectEntity, manager.TemporalEntitiesIndex[(10, firstEffectEntity.Id)]);

            using (var secondEffectEntityReference = manager.CreateEntity())
            {
                var secondEffectEntity = secondEffectEntityReference.Referenced;
                var secondEffect = secondEffectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);

                secondEffect.ExpirationTick = 9;

                Assert.Same(secondEffectEntity, manager.TemporalEntitiesIndex.First());
                Assert.Same(firstEffectEntity, manager.TemporalEntitiesIndex.Last());
                Assert.Equal(2, manager.TemporalEntitiesIndex.Count);

                secondEffect.ExpirationTick = 11;

                Assert.Same(firstEffectEntity, manager.TemporalEntitiesIndex.First());
                Assert.Same(secondEffectEntity, manager.TemporalEntitiesIndex.Last());

                secondEffect.ExpirationTick = null;

                Assert.Same(firstEffectEntity, manager.TemporalEntitiesIndex.First());
                Assert.Same(firstEffectEntity, manager.TemporalEntitiesIndex.Last());
            }

            manager.Queue.ProcessQueue(manager);
        }

        Assert.Equal(0, manager.TemporalEntitiesIndex.Count);
    }
}
