using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Utils.MessagingECS;

public class EntityIndexTest
{
    [Fact(Skip = "Unused")]
    public void Index_is_updated()
    {
        var manager = TestHelper.CreateGameManager();

        GameEntity firstLevelEntity;
        LevelComponent level;
        using (var levelEntityReference = manager.CreateEntity())
        {
            firstLevelEntity = levelEntityReference.Referenced;
            level = firstLevelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);
        }

        GameEntity firstKnowledgeEntity;
        using (var firstPositionEntityReference = manager.CreateEntity())
        {
            var firstPositionEntity = firstPositionEntityReference.Referenced;
            firstPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
            var firstPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
            firstPosition.LevelId = firstLevelEntity.Id;
            firstPosition.LevelCell = new Point(2, 3);
            firstPositionEntity.Position = firstPosition;

            using (var firstKnowledgeEntityReference = manager.CreateEntity())
            {
                firstKnowledgeEntity = firstKnowledgeEntityReference.Referenced;
                var knowledge = firstKnowledgeEntity.AddComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
                knowledge.KnownEntityId = firstPositionEntity.Id;
                var firstKnowledgePosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
                firstKnowledgePosition.LevelId = firstLevelEntity.Id;
                firstKnowledgePosition.LevelCell = new Point(2, 3);
                firstKnowledgeEntity.Position = firstKnowledgePosition;

                Assert.Same(firstKnowledgeEntity, level.KnownActors.GetValueOrDefault(new Point(2, 3)));
            }
        }

        GameEntity secondKnowledgeEntity;
        using (var secondPositionEntityReference = manager.CreateEntity())
        {
            var secondPositionEntity = secondPositionEntityReference.Referenced;
            secondPositionEntity.AddComponent<ItemComponent>((int)EntityComponent.Item);
            var secondPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
            secondPosition.LevelId = firstLevelEntity.Id;
            secondPosition.LevelX = 2;
            secondPosition.LevelY = 3;
            secondPositionEntity.Position = secondPosition;

            using (var secondKnowledgeEntityReference = manager.CreateEntity())
            {
                secondKnowledgeEntity = secondKnowledgeEntityReference.Referenced;
                var knowledge = secondKnowledgeEntity.AddComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
                knowledge.KnownEntityId = secondPositionEntity.Id;
                var secondKnowledgePosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
                secondKnowledgePosition.LevelId = firstLevelEntity.Id;
                secondKnowledgePosition.LevelCell = new Point(2, 3);
                secondKnowledgeEntity.Position = secondKnowledgePosition;

                Assert.Same(secondKnowledgeEntity, level.KnownItems.GetValueOrDefault(new Point(2, 3)));
                Assert.Same(firstKnowledgeEntity, level.KnownActors.GetValueOrDefault(new Point(2, 3)));
            }
        }

        firstKnowledgeEntity.Position.LevelCell = new Point(1, 3);

        Assert.Same(firstKnowledgeEntity, level.KnownActors.GetValueOrDefault(new Point(1, 3)));
        Assert.Same(secondKnowledgeEntity, level.KnownActors.GetValueOrDefault(new Point(2, 3)));

        secondKnowledgeEntity.Position = null;

        Assert.Null(level.KnownActors.GetValueOrDefault(new Point(2, 3)));

        using (var secondLevelEntityReference = manager.CreateEntity())
        {
            var secondLevelEntity = secondLevelEntityReference.Referenced;
            secondLevelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);
            firstKnowledgeEntity.Position.LevelId = secondLevelEntity.Id;

            Assert.Null(level.KnownActors.GetValueOrDefault(new Point(1, 3)));
            Assert.Same(firstKnowledgeEntity, level.KnownActors.GetValueOrDefault(new Point(1, 3)));

            secondLevelEntity.Level = null;
        }

        Assert.Null(firstKnowledgeEntity.Position);
    }
}
