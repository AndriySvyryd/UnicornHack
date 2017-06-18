using System;
using System.Collections.Generic;
using System.Linq;
using CSharpScriptSerialization;
using UnicornHack.Generation;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Creature : Actor
    {
        #region State

        private string _corpseVariantName;
        private ActorNoiseType? _noise;

        public byte InitialLevel { get; set; }
        public virtual Weight GenerationWeight { get; set; }
        public GenerationFlags GenerationFlags { get; set; }
        public Frequency GenerationFrequency { get; set; }
        public MonsterBehavior Behavior { get; set; }
        public short Alignment { get; set; }

        public ActorNoiseType Noise
        {
            get => _noise ?? (BaseActor as Creature)?.Noise ?? ActorNoiseType.Silent;
            set => _noise = value;
        }

        public string CorpseName
        {
            get => _corpseVariantName ?? (BaseActor as Creature)?.CorpseName;
            set => _corpseVariantName = value;
        }

        public Creature Corpse => CorpseName == null ? null : Loader.Get(CorpseName);

        public string PreviousStageName { get; set; }
        public Creature PreviousStage => PreviousStageName == null ? null : Loader.Get(PreviousStageName);

        public string NextStageName { get; set; }
        public Creature NextStage => NextStageName == null ? null : Loader.Get(NextStageName);

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

            creature.MaxHP = 1 + creature.XPLevel * 4;
            creature.MaxHP = creature.MaxHP < 1 ? 1 : creature.MaxHP;
            creature.HP = creature.MaxHP;

            creature.RecalculateEffectsAndAbilities();
            return creature;
        }

        protected override Actor CreateInstance(Game game) => new Creature(game);

        #endregion

        #region Actions

        public override bool Act()
        {
            if (TryAttackPlayerCharacter())
            {
                return true;
            }

            if (MovementDelay == 0)
            {
                NextActionTick += DefaultActionDelay;
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
                NextActionTick += DefaultActionDelay;
                return true;
            }
            var directionIndex = Game.Random.Next(minValue: 0, maxValue: possibleDirectionsToMove.Count);

            var targetCell = ToLevelCell(possibleDirectionsToMove[directionIndex]);
            if (targetCell != null)
            {
                return Move(targetCell.Value, safe: true);
            }

            return true;
        }

        private bool TryAttackPlayerCharacter()
        {
            var playerCharacter = Level.Players.FirstOrDefault(pc =>
                Level.GridDistance(this, pc) <= 1
                && pc.IsAlive);
            if (playerCharacter == null)
            {
                return false;
            }

            Attack(playerCharacter);
            return true;
        }

        private bool TryMoveToPlayerCharacter()
        {
            var playerCharacter = Level.Players.FirstOrDefault(pc =>
                Level.GridDistance(this, pc) <= 6
                && pc.IsAlive);
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

        private Func<string, byte, int, float> _weightFunction;

        public virtual float GetWeight(Level level)
        {
            if (_weightFunction == null)
            {
                _weightFunction = (GenerationWeight ?? new DefaultWeight()).CreateCreatureWeightFunction();
            }

            return _weightFunction(level.Branch.Name, level.Depth, 0);
        }

        #endregion

        #region Serialization

        public static readonly GroupedCSScriptLoader<byte, Creature> Loader =
            new GroupedCSScriptLoader<byte, Creature>(@"data\creatures\", c => c.InitialLevel);

        private static byte? _maxLevel;

        public static byte MaxLevel
        {
            get
            {
                if (_maxLevel == null)
                {
                    _maxLevel = Loader.GetAllKeys().Max();
                }
                return _maxLevel.Value;
            }
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
            propertyConditions.Add(nameof(Noise),
                (o, v) => (ActorNoiseType)v != ((o.BaseActor as Creature)?.Noise ?? ActorNoiseType.Silent));
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;

        #endregion
    }
}