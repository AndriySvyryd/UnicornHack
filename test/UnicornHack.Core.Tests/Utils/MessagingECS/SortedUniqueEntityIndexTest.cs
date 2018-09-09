using System.Linq;
using UnicornHack.Systems.Effects;
using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class SortedUniqueEntityIndexTest
    {
        [Fact]
        public void Index_is_updated()
        {
            var manager = TestHelper.CreateGameManager();

            Assert.Empty(manager.TemporalEntitiesIndex);

            using (var firstEffectEntityReference = manager.CreateEntity())
            {
                var firstEffectEntity = firstEffectEntityReference.Referenced;
                var firstEffect = firstEffectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);

                firstEffect.ExpirationTick = 10;

                Assert.Same(firstEffectEntity, manager.TemporalEntitiesIndex.Single());
                Assert.Same(firstEffectEntity, manager.TemporalEntitiesIndex[(10, firstEffectEntity.Id)]);

                using (var secondEffectEntityReference = manager.CreateEntity())
                {
                    var secondEffectEntity = secondEffectEntityReference.Referenced;
                    var secondEffect = secondEffectEntity.AddComponent<EffectComponent>((int)EntityComponent.Effect);

                    secondEffect.ExpirationTick = 9;

                    Assert.Same(secondEffectEntity, manager.TemporalEntitiesIndex.First());
                    Assert.Same(firstEffectEntity, manager.TemporalEntitiesIndex.Last());
                    Assert.Equal(2, manager.TemporalEntitiesIndex.Count());

                    secondEffect.ExpirationTick = 11;

                    Assert.Same(secondEffectEntity, manager.TemporalEntitiesIndex.Last());
                    Assert.Same(firstEffectEntity, manager.TemporalEntitiesIndex.First());

                    secondEffect.ExpirationTick = null;

                    Assert.Same(firstEffectEntity, manager.TemporalEntitiesIndex.Single());
                }
            }

            Assert.Empty(manager.TemporalEntitiesIndex);
        }
    }
}
