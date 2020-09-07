using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
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

            var length = levelComponent.TileCount;
            levelComponent.Terrain = ArrayPool<byte>.Shared.Rent(length);
            levelComponent.WallNeighbors = ArrayPool<byte>.Shared.Rent(length);
            levelComponent.VisibleNeighbours = ArrayPool<byte>.Shared.Rent(length);
            levelComponent.VisibleTerrain = ArrayPool<byte>.Shared.Rent(length);
            levelComponent.KnownTerrain = ArrayPool<byte>.Shared.Rent(length);

            Array.Clear(levelComponent.Terrain, 0, length);
            Array.Clear(levelComponent.WallNeighbors, 0, length);
            Array.Clear(levelComponent.VisibleNeighbours, 0, length);
            Array.Clear(levelComponent.VisibleTerrain, 0, length);
            for (var i = 0; i < length; i++)
            {
                levelComponent.KnownTerrain[i] = (byte)MapFeature.Unexplored;
            }

            levelComponent.EnsureInitialized();

            var rooms = fragment.Layout.Fill(levelComponent, fragment);
            fragment.CreatureGenerator.Fill(levelComponent, rooms.Where(r => !(r.Fragment is ConnectingMapFragment)).ToList());
            fragment.ItemGenerator.Fill(levelComponent, rooms);

            return rooms;
        }
    }
}
