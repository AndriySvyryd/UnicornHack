using System;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Generation.Map;
using UnicornHack.Services;
using UnicornHack.Services.English;
using UnicornHack.Utils;

namespace UnicornHack
{
    public static class TestHelper
    {
        public static Level CreateLevel(byte height = 10, byte width = 10, Game game = null)
        {
            var level = new Level
            {
                Height = height,
                Width = width,
                Game = game ?? CreateGame()
            };
            level.Terrain = new byte[level.Height * level.Width];
            level.WallNeighbours = new byte[level.Height * level.Width];
            level.VisibleTerrain = new byte[level.Height * level.Width];
            level.VisibleNeighbours = new byte[level.Height * level.Width];
            level.EnsureInitialized();

            return level;
        }

        public static Game CreateGame()
            => new Game
            {
                Services = new GameServices(new EnglishLanguageService(), new MemoryCache(new MemoryCacheOptions()))
            };

        public static Level BuildLevel(string map, int seed)
        {
            var game = CreateGame();
            game.Random = new SimpleRandom { Seed = seed };
            game.InitialSeed = seed;
            game.Repository = new TestRepository();

            var fragment = new NormalMapFragment
            {
                Map = map
            };
            fragment.EnsureInitialized(game);

            var level = CreateLevel(fragment.Height, fragment.Width, game);
            level.GenerationRandom = new SimpleRandom {Seed = seed};

            fragment.TryPlace(level, level.BoundingRectangle);
            return level;
        }

        public static string PrintMap(Level level, byte[] visibleTerrain = null)
        {
            var builder = new StringBuilder();
            var i = 0;
            visibleTerrain = visibleTerrain ?? level.VisibleTerrain;
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