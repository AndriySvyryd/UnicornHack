using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameState
{
    public class Level
    {
        public static readonly byte Height = 20;
        public static readonly byte Width = 80;
        public static string MainBranchName = "Cave of the Unknown";

        // TODO: These could be precalculated statically
        public static int[,] PointToIndex;
        public static Point[] IndexToPoint;

        private PathFinder _pathFinder;

        public Level()
        {
            _canMoveDelegate = CanMove;
        }

        public Level(Game game, string branchName, short depth)
            : this()
        {
            Id = game.NextLevelId++;
            Game = game;
            Name = branchName;
            Depth = depth;
            LastTurn = game.CurrentTurn;
        }

        static Level()
        {
            PointToIndex = new int[Width, Height];
            IndexToPoint = new Point[Width * Height];
            var i = 0;
            for (byte y = 0; y < Height; y++)
            {
                for (byte x = 0; x < Width; x++)
                {
                    PointToIndex[x, y] = i;
                    IndexToPoint[i++] = new Point(x, y);
                }
            }
        }

        public virtual int Id { get; private set; }
        public virtual string Name { get; set; }
        public virtual short Depth { get; set; }
        
        public virtual byte[] Layout { get; set; }
        public virtual int LastTurn { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int GameId { get; private set; }
        public virtual Game Game { get; set; }
        public virtual ICollection<Item> Items { get; private set; } = new HashSet<Item>();
        public virtual ICollection<Actor> Actors { get; private set; } = new HashSet<Actor>();
        public virtual ICollection<Stairs> UpStairs { get; private set; } = new HashSet<Stairs>();
        public virtual ICollection<Stairs> DownStairs { get; private set; } = new HashSet<Stairs>();

        [NotMapped]
        public virtual IEnumerable<PlayerCharacter> PlayerCharacters => Actors.OfType<PlayerCharacter>();

        // TODO: Adjust based on end game
        [NotMapped]
        public virtual short Difficulty => Depth;

        public virtual Actor Turn()
        {
            for (; LastTurn < Game.CurrentTurn; LastTurn++)
            {
                foreach (var actor in Actors
                    .OrderBy(a => a.Id)
                    .ToList())
                {
                    if (Game.ActingActor != null)
                    {
                        if (actor != Game.ActingActor)
                        {
                            continue;
                        }
                        Game.ActingActor = null;
                        Game.ActingActorId = null;
                    }

                    if (actor.Level != this)
                    {
                        continue;
                    }

                    if (actor.CanAct()
                        && !actor.Act())
                    {
                        return actor;
                    }
                }
            }

            return null;
        }

        public virtual byte GridDistance(Actor origin, Actor target)
        {
            var xDelta = target.LevelX - origin.LevelX;
            var yDelta = target.LevelY - origin.LevelY;
            return (byte)Math.Max(Math.Abs(xDelta), Math.Abs(yDelta));
        }

        public virtual Vector? GetFirstStepFromShortestPath(Actor origin, Actor target)
        {
            var firstPoint = new Point(origin.LevelX, origin.LevelY);
            var nextPoint = new Point(target.LevelX, target.LevelY);

            var path = GetShortestPath(origin, target);
            if (path == null)
            {
                return null;
            }

            if (path.Count != 0)
            {
                nextPoint = path[path.Count - 1];
            }

            Debug.Assert(firstPoint.DistanceTo(nextPoint) <= 1);

            return firstPoint.DirectionTo(nextPoint);
        }

        public virtual List<Point> GetShortestPath(Actor origin, Actor target)
        {
            if (_pathFinder == null)
            {
                _pathFinder = new PathFinder(_canMoveDelegate, PointToIndex, IndexToPoint);
            }

            var firstPoint = new Point(origin.LevelX, origin.LevelY);
            var nextPoint = new Point(target.LevelX, target.LevelY);

            return _pathFinder.FindPath(firstPoint, nextPoint);
        }

        private readonly Func<Point, int, int?> _canMoveDelegate;

        private int? CanMove(Point currentLocation, int directionIndex)
        {
            var direction = MovementDirections[directionIndex];
            var newLocationX = (byte)(currentLocation.X + direction.X);
            var newLocationY = (byte)(currentLocation.Y + direction.Y);

            // Since byte is unsigned there is no need to compare with 0
            if ((newLocationX >= Width) || (newLocationY >= Height))
            {
                return null;
            }

            var newLocationIndex = PointToIndex[newLocationX, newLocationY];

            // TODO: Use locomotion type
            // TODO: block if directionIndex > 3 (diagonal) and path is too narrow to squeeze through
            // TODO: Also avoid actors (at least adjacent ones)
            if (!((MapFeature)Layout[newLocationIndex]).CanMoveTo())
            {
                return null;
            }

            return newLocationIndex;
        }

        public virtual IReadOnlyList<Vector> GetPossibleMovementDirections(Point currentLocation, bool allowZ, bool safe)
        {
            var availableDirections = new List<Vector>();
            for (var i = 0; i < (allowZ ? 10 : 8); i++)
            {
                if (CanMove(currentLocation, i) == null)
                {
                    continue;
                }

                var direction = MovementDirections[i];
                if (direction.Z == 0
                    && Actors.Any(a =>
                        (a.LevelX == currentLocation.X + direction.X) &&
                        (a.LevelY == currentLocation.Y + direction.Y)))
                {
                    continue;
                }

                availableDirections.Add(MovementDirections[i]);
            }

            return availableDirections;
        }

        public static Level CreateLevel(Game game, string branchName, short depth)
        {
            var level = new Level(game, branchName, depth);
            game.Levels.Add(level);
            var layout = StaticLevels[depth][branchName];
            var byteLayout = new byte[Height * Width];
            byte x = 0;
            byte y = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < layout.Length; i++)
            {
                var mapPoint = layout[i];
                var feature = MapFeature.Default;
                switch (mapPoint)
                {
                    case ' ':
                        break;
                    case '·':
                        feature = MapFeature.Floor;
                        break;
                    case '│':
                        feature = MapFeature.WallVertical;
                        break;
                    case '─':
                        feature = MapFeature.WallHorizontal;
                        break;
                    case '┌':
                        feature = MapFeature.WallTopLeft;
                        break;
                    case '┐':
                        feature = MapFeature.WallTopRight;
                        break;
                    case '└':
                        feature = MapFeature.WallBottomLeft;
                        break;
                    case '┘':
                        feature = MapFeature.WallBottomRight;
                        break;
                    case '┼':
                        feature = MapFeature.WallCross;
                        break;
                    case '┴':
                        feature = MapFeature.WallTUp;
                        break;
                    case '┬':
                        feature = MapFeature.WallTDown;
                        break;
                    case '┤':
                        feature = MapFeature.WallTLeft;
                        break;
                    case '├':
                        feature = MapFeature.WallTRight;
                        break;
                    case '#': //▒
                        feature = MapFeature.Corridor;
                        break;
                    case '≈':
                        feature = MapFeature.Pool;
                        break;
                    case '<':
                        feature = MapFeature.Floor;
                        level.UpStairs.Add(Stairs.CreateUpStairs(game, level, x, y));
                        break;
                    case '>':
                        feature = MapFeature.Floor;
                        level.DownStairs.Add(Stairs.CreateDownStairs(game, level, x, y));
                        break;
                    case '$':
                        feature = MapFeature.Floor;
                        level.Items.Add(new StackableItem(ItemType.Gold, quantity: 9, x: x, y: y, level: level));
                        break;
                    case '%':
                        feature = MapFeature.Floor;
                        level.Items.Add(new StackableItem(ItemType.Food, quantity: 1, x: x, y: y, level: level));
                        break;
                    case ')':
                        feature = MapFeature.Floor;
                        break;
                    case '[':
                        feature = MapFeature.Floor;
                        break;
                    case 'b':
                        feature = MapFeature.Floor;
                        Monster.CreateMonster(MonsterVariant.BugLighting, x, y, level);
                        break;
                    case 'B':
                        feature = MapFeature.Floor;
                        Monster.CreateMonster(MonsterVariant.FireFly, x, y, level);
                        break;
                    case '\r':
                        continue;
                    case '\n':
                        if (x != Width)
                        {
                            throw new InvalidOperationException($"The width of line {y} is {x}, but expected {Width}");
                        }
                        x = 0;
                        y++;
                        continue;
                }

                byteLayout[x + Width*y] = (byte)feature;
                x++;
            }

            if (y + 1 != Height)
            {
                throw new InvalidOperationException($"The height is {y + 1}, but expected {Height}");
            }
            
            level.Layout = byteLayout;
            return level;
        }

        public static Level CreateUpLevel(Stairs stairs)
        {
            var level = CreateLevel(stairs.Down.Game, stairs.BranchName, (short)(stairs.Down.Depth - 1));
            var connectingStairs = level.DownStairs.Single(s => s.BranchName == stairs.Down.Name);
            stairs.Up = level;
            stairs.UpLevelX = connectingStairs.UpLevelX;
            stairs.UpLevelY = connectingStairs.UpLevelY;
            level.DownStairs.Add(stairs);
            level.DownStairs.Remove(connectingStairs);
            return level;
        }

        public static Level CreateDownLevel(Stairs stairs)
        {
            var level = CreateLevel(stairs.Up.Game, stairs.BranchName, (short)(stairs.Up.Depth + 1));
            var connectingStairs = level.UpStairs.Single(s => s.BranchName == stairs.Up.Name);
            stairs.Down = level;
            stairs.DownLevelX = connectingStairs.DownLevelX;
            stairs.DownLevelY = connectingStairs.DownLevelY;
            level.UpStairs.Add(stairs);
            level.UpStairs.Remove(connectingStairs);
            return level;
        }

        // Order matters
        public static readonly Vector[] MovementDirections =
        {
            new Vector(x: 0, y: -1, z: 0), new Vector(x: 1, y: 0, z: 0),
            new Vector(x: 0, y: 1, z: 0), new Vector(-1, y: 0, z: 0),
            new Vector(x: 1, y: -1, z: 0), new Vector(x: 1, y: 1, z: 0),
            new Vector(-1, y: 1, z: 0), new Vector(-1, -1, z: 0),
            new Vector(x: 0, y: 0, z: -1), new Vector(x: 0, y: 0, z: 1)
        };

        public static readonly int DefaultMovementCost = 12;

        // ReSharper disable ArgumentsStyleStringLiteral
        private static readonly Dictionary<int, Dictionary<string, string>> StaticLevels =
            new Dictionary<int, Dictionary<string, string>>
            {
                {
                    0, new Dictionary<string, string>
                    {
                        {
                            MainBranchName,
                            @"                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                         ┌─┐                                    
                                         │>│                                    
                                         └─┘                                    
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                "
                        }
                    }
                },
                {
                    1, new Dictionary<string, string>
                    {
                        {
                            MainBranchName,
                            @"                                                                                
                     ┌─────────┐                 ┌─────┐                        
                     │··········#################····%··##                      
 ┌──────┐            │··≈······│##   ┌───┐#      │·····│ #                      
 │······│            │··········#### │·B·│#      │·····│ ###                    
 │······│            │·········│#  # │>···#      └─·───┘   #    ┌─────────┐     
 │······│  #         └───────·─┘#  ##····│######################··········│     
 │·······# ######################   #└───┘#        ###       #  │······b··│     
 │···b··│#                   #      ####  #          #       ###│·········│     
 │·$····│#                   #         #  #          ###       #·······[··│     
 └─────·┘###                 #         ####            #        │·········│     
       ####################################################     └─────────┘     
           ### ┌─────────────·┐           # ┌──────────·───┐                    
             # │···············########   # │··············│                    
             ##····b··········│           # │$··········<··│                    
               │········)·····│           ##···············│                    
               │···%··········│             └──────────────┘                    
               └──────────────┘                                                 
                    #                                                           
                                                                                "
                        }
                    }
                },
                {
                    2, new Dictionary<string, string>
                    {
                        {
                            MainBranchName,
                            @"                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                    ┌───┐                                       
                                    │BBB│                                       
                                    │B<B│                                       
                                    │BBB│                                       
                                    └───┘                                       
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                
                                                                                "
                        }
                    }
                }
            };

        // ReSharper restore ArgumentsStyleStringLiteral
    }
}