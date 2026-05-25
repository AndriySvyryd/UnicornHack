using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Beings;
using UnicornHack.Systems.Items;
using UnicornHack.Systems.Knowledge;
using UnicornHack.Systems.Levels;

namespace UnicornHack.Utils.MessagingECS;

public class EntityIndexTest
{
    [Fact]
    public void Index_is_updated()
    {
        var manager = TestHelper.CreateGameManager();

        GameEntity firstLevelEntity;
        RoomComponent level;
        using (var levelEntityReference = manager.CreateEntity())
        {
            firstLevelEntity = levelEntityReference.Referenced;
            level = firstLevelEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);
        }

        GameEntity firstKnowledgeEntity;
        using (var firstPositionEntityReference = manager.CreateEntity())
        {
            var firstPositionEntity = firstPositionEntityReference.Referenced;
            firstPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
            firstPositionEntity.AddComponent<BeingComponent>((int)EntityComponent.Being);
            var firstPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
            firstPosition.RoomId = firstLevelEntity.Id;
            firstPosition.Cell = new Point(2, 3);
            firstPositionEntity.Position = firstPosition;

            using (var firstKnowledgeEntityReference = manager.CreateEntity())
            {
                firstKnowledgeEntity = firstKnowledgeEntityReference.Referenced;
                var knowledge = firstKnowledgeEntity.AddComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
                knowledge.KnownEntityId = firstPositionEntity.Id;
                var firstKnowledgePosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
                firstKnowledgePosition.RoomId = firstLevelEntity.Id;
                firstKnowledgePosition.Cell = new Point(2, 3);
                firstKnowledgeEntity.Position = firstKnowledgePosition;

                Assert.Same(firstKnowledgeEntity, level.KnownActors.GetValueOrDefault(new Point(2, 3)));
            }
        }

        GameEntity secondKnowledgeEntity;
        using (var secondPositionEntityReference = manager.CreateEntity())
        {
            var secondPositionEntity = secondPositionEntityReference.Referenced;
            secondPositionEntity.AddComponent<ItemComponent>((int)EntityComponent.Item);
            secondPositionEntity.AddComponent<PhysicalComponent>((int)EntityComponent.Physical);
            var secondPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
            secondPosition.RoomId = firstLevelEntity.Id;
            secondPosition.CellX = 2;
            secondPosition.CellY = 3;
            secondPositionEntity.Position = secondPosition;

            using (var secondKnowledgeEntityReference = manager.CreateEntity())
            {
                secondKnowledgeEntity = secondKnowledgeEntityReference.Referenced;
                var knowledge = secondKnowledgeEntity.AddComponent<KnowledgeComponent>((int)EntityComponent.Knowledge);
                knowledge.KnownEntityId = secondPositionEntity.Id;
                var secondKnowledgePosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
                secondKnowledgePosition.RoomId = firstLevelEntity.Id;
                secondKnowledgePosition.Cell = new Point(2, 3);
                secondKnowledgeEntity.Position = secondKnowledgePosition;

                Assert.Same(secondKnowledgeEntity, level.KnownItems.GetValueOrDefault(new Point(2, 3)));
                Assert.Same(firstKnowledgeEntity, level.KnownActors.GetValueOrDefault(new Point(2, 3)));
            }
        }

        firstKnowledgeEntity.Position.Cell = new Point(1, 3);

        Assert.Same(firstKnowledgeEntity, level.KnownActors.GetValueOrDefault(new Point(1, 3)));
        Assert.Same(secondKnowledgeEntity, level.KnownItems.GetValueOrDefault(new Point(2, 3)));

        secondKnowledgeEntity.Position = null;

        Assert.Null(level.KnownItems.GetValueOrDefault(new Point(2, 3)));

        using (var secondLevelEntityReference = manager.CreateEntity())
        {
            var secondLevelEntity = secondLevelEntityReference.Referenced;
            var secondLevel = secondLevelEntity.AddComponent<RoomComponent>((int)EntityComponent.Room);
            firstKnowledgeEntity.Position.RoomId = secondLevelEntity.Id;

            Assert.Null(level.KnownActors.GetValueOrDefault(new Point(1, 3)));
            Assert.Same(firstKnowledgeEntity, secondLevel.KnownActors.GetValueOrDefault(new Point(1, 3)));

            secondLevelEntity.Room = null;
        }

        Assert.Null(firstKnowledgeEntity.Position);
    }
}
