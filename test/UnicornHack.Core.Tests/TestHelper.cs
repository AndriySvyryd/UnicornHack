using System;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Generation;
using UnicornHack.Generation.Map;
using UnicornHack.Primitives;
using UnicornHack.Services;
using UnicornHack.Services.English;
using UnicornHack.Systems.Levels;
using UnicornHack.Utils;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack
{
    public static class TestHelper
    {
        public static Game CreateGame(uint? seed = null)
        {
            var game = new Game
            {
                Random = new SimpleRandom {Seed = seed ?? (uint)Environment.TickCount},
                InitialSeed = seed,
                Repository = new TestRepository(),
                Services = new GameServices(new EnglishLanguageService(), new MemoryCache(new MemoryCacheOptions()))
            };

            game.Manager = CreateGameManager(game);

            return game;
        }

        public static GameManager CreateGameManager(Game game = null)
        {
            var manager = new GameManager {Game = game ?? CreateGame()};
            manager.Initialize(new SequentialMessageQueue<GameManager>());
            return manager;
        }

        public static LevelComponent BuildLevel(string map = "", uint? seed = null)
        {
            var game = CreateGame(seed);

            var fragment = new DefiningMapFragment
            {
                Map = map,
                Layout = new EmptyLayout {Coverage = 0},
                CreatureGenerator = new CreatureGenerator {ExpectedInitialCount = 0},
                ItemGenerator = new ItemGenerator {ExpectedInitialCount = 0}
            };

            var level = CreateLevel(fragment, game);
            level.GenerationRandom = new SimpleRandom {Seed = game.Random.Seed};

            fragment.TryPlace(level, level.BoundingRectangle);
            return level;
        }

        public static LevelComponent CreateLevel(DefiningMapFragment fragment, Game game)
        {
            var branch = new GameBranch
            {
                Game = game,
                Name = "test branch",
                Length = 1,
                Difficulty = 1
            };
            game.Branches.Add(branch);

            fragment.EnsureInitialized(game);
            if (fragment.Height != 0)
            {
                fragment.LevelHeight = fragment.Height;
            }

            if (fragment.Width != 0)
            {
                fragment.LevelWidth = fragment.Width;
            }

            var level = LevelGenerator.CreateEmpty(branch, 0, game.Random.Seed, game.Manager);
            LevelGenerator.Generate(level, fragment);

            return level;
        }


        public static string PrintMap(LevelComponent level, byte[] visibleTerrain)
        {
            var builder = new StringBuilder();
            var i = 0;
            for (var y = 0; y < level.Height; y++)
            {
                for (var x = 0; x < level.Width; x++)
                {
                    var isVisible = visibleTerrain[i];
                    if (isVisible == 0)
                    {
                        builder.Append(' ');
                    }
                    else
                    {
                        var feature = (MapFeature)level.Terrain[i];
                        var symbol = ' ';
                        switch (feature)
                        {
                            case MapFeature.Default:
                                break;
                            case MapFeature.StoneFloor:
                                symbol = '·';
                                break;
                            case MapFeature.RockFloor:
                                symbol = '█';
                                break;
                            case MapFeature.StoneWall:
                                var neighbours = (DirectionFlags)level.WallNeighbours[i] & DirectionFlags.Cross;
                                switch (neighbours)
                                {
                                    case DirectionFlags.None:
                                        symbol = '●';
                                        break;
                                    case DirectionFlags.North:
                                        symbol = '╹';
                                        break;
                                    case DirectionFlags.East:
                                        symbol = '╺';
                                        break;
                                    case DirectionFlags.NorthAndEast:
                                        symbol = '┗';
                                        break;
                                    case DirectionFlags.South:
                                        symbol = '╻';
                                        break;
                                    case DirectionFlags.Longitudinal:
                                        symbol = '┃';
                                        break;
                                    case DirectionFlags.SouthAndEast:
                                        symbol = '┏';
                                        break;
                                    case DirectionFlags.NorthEastSouth:
                                        symbol = '┣';
                                        break;
                                    case DirectionFlags.West:
                                        symbol = '╸';
                                        break;
                                    case DirectionFlags.NorthAndWest:
                                        symbol = '┛';
                                        break;
                                    case DirectionFlags.Latitudinal:
                                        symbol = '━';
                                        break;
                                    case DirectionFlags.NorthEastWest:
                                        symbol = '┻';
                                        break;
                                    case DirectionFlags.SouthAndWest:
                                        symbol = '┓';
                                        break;
                                    case DirectionFlags.NorthWestSouth:
                                        symbol = '┫';
                                        break;
                                    case DirectionFlags.SouthEastWest:
                                        symbol = '┳';
                                        break;
                                    case DirectionFlags.Cross:
                                        symbol = '╋';
                                        break;
                                    default:
                                        throw new InvalidOperationException("Invalid wall neighbours: " + neighbours);
                                }

                                break;
                            case MapFeature.StoneArchway:
                                symbol = '∩';
                                break;
                            case MapFeature.Pool:
                                symbol = '≈';
                                break;
                            default:
                                throw new NotSupportedException($"Map feature {feature} not supported.");
                        }

                        builder.Append(symbol);
                    }

                    i++;
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
