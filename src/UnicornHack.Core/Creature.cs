using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpScriptSerialization;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Creature : Actor
    {
        #region State

        private string _corpseVariantName;
        private ActorNoiseType? _noise;

        public byte InitialLevel { get; set; }
        public GenerationFlags GenerationFlags { get; set; }
        public Frequency GenerationFrequency { get; set; }
        public MonsterBehavior Behavior { get; set; }
        public short Alignment { get; set; }

        public ActorNoiseType Noise
        {
            get { return _noise ?? (BaseActor as Creature)?.Noise ?? ActorNoiseType.Silent; }
            set { _noise = value; }
        }

        public string CorpseName
        {
            get { return _corpseVariantName ?? (BaseActor as Creature)?.CorpseName ?? null; }
            set { _corpseVariantName = value; }
        }

        public Creature Corpse => CorpseName == null ? null : Get(CorpseName);

        public string PreviousStageName { get; set; }
        public Creature PreviousStage => PreviousStageName == null ? null : Get(PreviousStageName);

        public string NextStageName { get; set; }
        public Creature NextStage => NextStageName == null ? null : Get(NextStageName);

        #endregion

        #region Creation

        public Creature()
        {
        }

        public Creature(Game game)
            : base(game)
        {
        }

        public override Actor Instantiate(Level level, byte x, byte y)
        {
            var creature = (Creature)base.Instantiate(level, x, y);
            creature.Noise = Noise;
            creature.Alignment = Alignment;
            creature.Behavior = Behavior;
            creature.CorpseName = CorpseName;
            creature.PreviousStageName = PreviousStageName;
            creature.NextStageName = NextStageName;
            creature.InitialLevel = InitialLevel;
            creature.XPLevel = InitialLevel;

            creature.AdjustInitialXPLevel();
            creature.RecalculateEffectsAndAbilities();
            return creature;
        }

        protected override Actor CreateInstance(Game game) => new Creature(game);

        public virtual void AdjustInitialXPLevel()
        {
            if (XPLevel < 50)
            {
                var relativeLevelDifficulty = Level.Difficulty - XPLevel;
                if (relativeLevelDifficulty < 0)
                {
                    XPLevel--;
                }
                else
                {
                    XPLevel += (byte)(relativeLevelDifficulty/5);
                }

                var playerLevel = Game.Players.OrderByDescending(pc => pc.XPLevel).FirstOrDefault()?.XPLevel ??
                                  1;
                var relativePlayerLevel = playerLevel - XPLevel;
                if (relativePlayerLevel > 0)
                {
                    XPLevel += (byte)(relativePlayerLevel/4);
                }

                while (PreviousStage != null
                       && XPLevel < InitialLevel)
                {
                    BaseName = PreviousStage.Name;
                }

                while (NextStage != null
                       && XPLevel >= NextStage.InitialLevel)
                {
                    BaseName = NextStage.Name;
                }

                XPLevel = XPLevel < 1 ? (byte)1 : XPLevel;
                var upperLimit = (byte)Math.Min(3*InitialLevel/2, 49);
                XPLevel = XPLevel > upperLimit ? upperLimit : XPLevel;
            }

            MaxHP = 1 + XPLevel*4;

            MaxHP = MaxHP < 1 ? 1 : MaxHP;
            HP = MaxHP;
        }

        #endregion

        #region Actions

        public override bool Act()
        {
            while (ActionPoints >= ActionPointsPerTurn)
            {
                if (TryAttackPlayerCharacter())
                {
                    return true;
                }

                if (TryMoveToPlayerCharacter())
                {
                    return true;
                }

                var possibleDirectionsToMove = Level.GetPossibleMovementDirections(
                    new Point(LevelX, LevelY), safe: true);
                if (possibleDirectionsToMove.Count == 0)
                {
                    break;
                }
                var directionIndex = Game.NextRandom(minValue: 0, maxValue: possibleDirectionsToMove.Count);

                var targetCell = ToLevelCell(possibleDirectionsToMove[directionIndex]);
                if (targetCell != null)
                {
                    return Move(targetCell.Value, safe: true);
                }
            }

            return true;
        }

        private bool TryAttackPlayerCharacter()
        {
            var playerCharacter = Game.Players.FirstOrDefault(pc =>
                Level.GridDistance(this, pc) <= 1
                && pc.IsAlive
                && pc.Level == Level);
            if (playerCharacter == null)
            {
                return false;
            }

            Attack(playerCharacter);
            return true;
        }

        private bool TryMoveToPlayerCharacter()
        {
            var playerCharacter = Game.Players.FirstOrDefault(pc =>
                Level.GridDistance(this, pc) <= 6
                && pc.IsAlive
                && pc.Level == Level);
            if (playerCharacter == null)
            {
                return false;
            }

            var direction = Level.GetFirstStepFromShortestPath(this, playerCharacter);
            if (direction != null)
            {
                var targetCell = ToLevelCell(direction.Value);
                if (targetCell != null
                    && Move(targetCell.Value, pretend: true, safe: true))
                {
                    return Move(targetCell.Value, safe: true);
                }
            }

            return false;
        }

        #endregion

        #region Serialization

        public static readonly string BasePath = Path.Combine(AppContext.BaseDirectory, @"data\creatures\");

        protected static Dictionary<string, Creature> NameLookup { get; } =
            new Dictionary<string, Creature>(StringComparer.Ordinal);

        private static bool _allLoaded;

        public static IEnumerable<Creature> GetAllCreatureVariants()
        {
            if (!_allLoaded)
            {
                foreach (var file in
                    Directory.EnumerateFiles(BasePath, "*" + CSScriptDeserializer.Extension,
                        SearchOption.AllDirectories))
                {
                    if (!NameLookup.ContainsKey(
                        CSScriptDeserializer.GetNameFromFilename(Path.GetFileNameWithoutExtension(file))))
                    {
                        Load(file);
                    }
                }
                _allLoaded = true;
            }

            return NameLookup.Values;
        }

        public new static Creature Get(string name)
        {
            Creature variant;
            if (NameLookup.TryGetValue(name, out variant))
            {
                return variant;
            }

            var path = Path.Combine(BasePath, CSScriptDeserializer.GetFilename(name));
            if (!File.Exists(path))
            {
                return null;
            }

            return Load(path);
        }

        private static Creature Load(string path)
        {
            var creature = CSScriptDeserializer.LoadFile<Creature>(path);
            NameLookup[creature.Name] = creature;
            return creature;
        }

        private static readonly CSScriptSerializer Serializer = new PropertyCSScriptSerializer<Creature>(
            GetPropertyConditions<Creature>());

        protected new static Dictionary<string, Func<TCreature, object, bool>> GetPropertyConditions<TCreature>()
            where TCreature : Creature
        {
            var propertyConditions = Actor.GetPropertyConditions<TCreature>();
            propertyConditions.Add(nameof(InitialLevel), (o, v) => (byte)v != 0);
            propertyConditions.Add(nameof(PreviousStageName), (o, v) => v != null);
            propertyConditions.Add(nameof(NextStageName), (o, v) => v != null);
            propertyConditions.Add(nameof(CorpseName), (o, v) => (string)v != (o.BaseActor as Creature)?.CorpseName);
            propertyConditions.Add(nameof(GenerationFlags), (o, v) => (GenerationFlags)v != GenerationFlags.None);
            propertyConditions.Add(nameof(GenerationFrequency), (o, v) => (Frequency)v != Frequency.Never);
            propertyConditions.Add(nameof(Behavior), (o, v) => (MonsterBehavior)v != MonsterBehavior.None);
            propertyConditions.Add(nameof(Alignment), (o, v) => (short)v != 0);
            propertyConditions.Add(nameof(Noise), (o, v) => (ActorNoiseType)v != ((o.BaseActor as Creature)?.Noise ?? ActorNoiseType.Silent));
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;

        #endregion
    }
}