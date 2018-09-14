using System;
using UnicornHack.Systems.Actors;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils.DataStructures;
using Xunit;

namespace UnicornHack.Utils.MessagingECS
{
    public class UniqueEntityIndexTest
    {
        [Fact]
        public void Index_is_updated()
        {
            var manager = TestHelper.CreateGameManager();
            using (var levelEntityReference = manager.CreateEntity())
            {
                var levelEntity = levelEntityReference.Referenced;
                levelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);

                using (var firstPositionEntityReference = manager.CreateEntity())
                {
                    var firstPositionEntity = firstPositionEntityReference.Referenced;
                    firstPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
                    var firstPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
                    firstPosition.LevelId = levelEntity.Id;
                    firstPosition.LevelCell = new Point(2, 3);
                    firstPositionEntity.Position = firstPosition;

                    Assert.Same(firstPositionEntity, manager.LevelActorToLevelCellIndex[(levelEntity.Id, 2, 3)]);

                    firstPosition.LevelCell = new Point(1, 2);

                    Assert.Same(firstPositionEntity, manager.LevelActorToLevelCellIndex[(levelEntity.Id, 1, 2)]);
                    Assert.Null(manager.LevelActorToLevelCellIndex[(levelEntity.Id, 2, 3)]);

                    using (var secondPositionEntityReference = manager.CreateEntity())
                    {
                        var secondPositionEntity = secondPositionEntityReference.Referenced;
                        secondPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
                        var secondPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
                        secondPosition.LevelId = levelEntity.Id;
                        secondPosition.LevelX = 2;
                        secondPosition.LevelY = 3;
                        secondPositionEntity.Position = secondPosition;

                        Assert.Same(secondPositionEntity, manager.LevelActorToLevelCellIndex[(levelEntity.Id, 2, 3)]);

                        secondPositionEntity.Position = null;

                        Assert.Null(manager.LevelActorToLevelCellIndex[(levelEntity.Id, 2, 3)]);
                    }

                    Assert.Same(firstPositionEntity, manager.LevelActorToLevelCellIndex[(levelEntity.Id, 1, 2)]);

                    using (var secondLevelEntityReference = manager.CreateEntity())
                    {
                        var secondLevelEntity = secondLevelEntityReference.Referenced;
                        secondLevelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);
                        firstPosition.LevelId = secondLevelEntity.Id;

                        Assert.Null(manager.LevelActorToLevelCellIndex[(levelEntity.Id, 1, 2)]);

                        secondLevelEntity.Level = null;
                    }

                    Assert.Null(firstPositionEntity.Position);
                }
            }
        }

        [Fact]
        public void Throws_on_conflict()
        {
            var manager = TestHelper.CreateGameManager();
            using (var levelEntityReference = manager.CreateEntity())
            {
                var levelEntity = levelEntityReference.Referenced;
                levelEntity.AddComponent<LevelComponent>((int)EntityComponent.Level);

                using (var firstPositionEntityReference = manager.CreateEntity())
                {
                    var firstPositionEntity = firstPositionEntityReference.Referenced;
                    firstPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
                    var firstPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
                    firstPosition.LevelId = levelEntity.Id;
                    firstPosition.LevelCell = new Point(2, 3);
                    firstPositionEntity.Position = firstPosition;

                    Assert.Same(firstPositionEntity, manager.LevelActorToLevelCellIndex[(levelEntity.Id, 2, 3)]);

                    using (var secondPositionEntityReference = manager.CreateEntity())
                    {
                        var secondPositionEntity = secondPositionEntityReference.Referenced;
                        secondPositionEntity.AddComponent<AIComponent>((int)EntityComponent.AI);
                        var secondPosition = manager.CreateComponent<PositionComponent>((int)EntityComponent.Position);
                        secondPosition.LevelId = levelEntity.Id;
                        secondPosition.LevelX = 2;
                        secondPosition.LevelY = 3;

                        Assert.Throws<InvalidOperationException>(() => secondPositionEntity.Position = secondPosition);
                    }
                }
            }
        }
    }
}
