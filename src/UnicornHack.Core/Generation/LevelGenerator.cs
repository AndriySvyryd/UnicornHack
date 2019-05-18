using System;
using System.Collections.Generic;
using UnicornHack.Generation.Map;
using UnicornHack.Primitives;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;

namespace UnicornHack.Generation
{
    public static class LevelGenerator
    {
        public static readonly byte MaxDifficulty = 30;

        public static LevelComponent CreateEmpty(GameBranch branch, byte depth, uint seed, GameManager manager)
        {
            if (branch.Length < depth)
            {
                throw new InvalidOperationException("Level created beyond branch length");
            }

            using (var levelEntityReference = manager.CreateEntity())
            {
                var level = manager.CreateComponent<LevelComponent>(EntityComponent.Level);
                level.BranchName = branch.Name;
                level.Depth = depth;
                level.Terrain = new byte[0];
                level.KnownTerrain = new byte[0];
                level.WallNeighbors = new byte[0];
                level.VisibleTerrain = new byte[0];
                level.VisibleNeighbours = new byte[0];
                level.VisibleTerrain = new byte[0];
                level.Difficulty = branch.Difficulty + depth;
                if (level.Difficulty > MaxDifficulty)
                {
                    throw new InvalidOperationException(
                        $"Difficulty {level.Difficulty} greater than max {MaxDifficulty}");
                }

                levelEntityReference.Referenced.Level = level;
                level.GenerationRandom = new SimpleRandom {Seed = seed};
                branch.Levels.Add(level);
                level.Branch = branch;

                return level;
            }
        }

        public static bool EnsureGenerated(LevelComponent levelComponent)
        {
            if (levelComponent.Width != 0)
            {
                return false;
            }

            try
            {
                var fragment = levelComponent.GenerationRandom.Pick(DefiningMapFragment.Loader.GetAsList(),
                    f => f.GetWeight(levelComponent.BranchName, levelComponent.Depth));

                return Generate(levelComponent, fragment) != null;
            }
            catch (Exception e)
            {
                // TODO: Log parameters if failed
                var msg = $@"Error while generating level '{levelComponent.BranchName}:{levelComponent.Depth
                    }', initial seed {levelComponent.Game.InitialSeed}:
";

                Console.WriteLine(msg);
                Console.WriteLine(e);
                throw new Exception(msg, e);
            }
        }

        public static List<Room> Generate(LevelComponent levelComponent, DefiningMapFragment fragment)
        {
            levelComponent.Height = fragment.LevelHeight;
            levelComponent.Width = fragment.LevelWidth;

            levelComponent.Terrain = new byte[levelComponent.Height * levelComponent.Width];
            levelComponent.WallNeighbors = new byte[levelComponent.Height * levelComponent.Width];
            levelComponent.VisibleNeighbours = new byte[levelComponent.Height * levelComponent.Width];
            levelComponent.VisibleTerrain = new byte[levelComponent.Height * levelComponent.Width];
            levelComponent.KnownTerrain = new byte[levelComponent.Height * levelComponent.Width];
            for (var i = 0; i < levelComponent.KnownTerrain.Length; i++)
            {
                levelComponent.KnownTerrain[i] = (byte)MapFeature.Unexplored;
            }

            levelComponent.EnsureInitialized();

            var rooms = fragment.Layout.Fill(levelComponent, fragment);
            fragment.CreatureGenerator.Fill(levelComponent, rooms);
            fragment.ItemGenerator.Fill(levelComponent, rooms);

            return rooms;
        }
    }
}
