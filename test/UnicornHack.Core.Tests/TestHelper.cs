using System;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Services;
using UnicornHack.Services.English;

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
                                var neighbour = level.WallNeighbours[i] & 0xF;
                                switch (neighbour)
                                {
                                    case 0:
                                        symbol = '●';
                                        break;
                                    case 1:
                                        symbol = '╹';
                                        break;
                                    case 2:
                                        symbol = '╺';
                                        break;
                                    case 3:
                                        symbol = '┗';
                                        break;
                                    case 4:
                                        symbol = '╻';
                                        break;
                                    case 5:
                                        symbol = '┃';
                                        break;
                                    case 6:
                                        symbol = '┏';
                                        break;
                                    case 7:
                                        symbol = '┣';
                                        break;
                                    case 8:
                                        symbol = '╸';
                                        break;
                                    case 9:
                                        symbol = '┛';
                                        break;
                                    case 10:
                                        symbol = '━';
                                        break;
                                    case 11:
                                        symbol = '┻';
                                        break;
                                    case 12:
                                        symbol = '┓';
                                        break;
                                    case 13:
                                        symbol = '┫';
                                        break;
                                    case 14:
                                        symbol = '┳';
                                        break;
                                    case 15:
                                        symbol = '╋';
                                        break;
                                    default:
                                        throw new InvalidOperationException($"Invalid wall neighbours: {neighbour}");
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