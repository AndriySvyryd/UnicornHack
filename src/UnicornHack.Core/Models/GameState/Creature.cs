using System;
using System.Collections.Generic;
using System.Linq;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameState
{
    public class Creature : Actor
    {
        public Creature()
        {
        }

        public Creature(CreatureVariant variant, byte x, byte y, Level level)
            : base(variant, x, y, level)
        {
            XPLevel = variant.InitialLevel;
        }

        public override ActorVariant Variant => CreatureVariant;

        public virtual CreatureVariant CreatureVariant => CreatureVariant.Get(PolymorphedVariant ?? OriginalVariant);

        public override byte MovementRate => Variant.MovementRate;

        public override IList<Ability> Abilities => CreatureVariant.Abilities;

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
                    new Point(LevelX, LevelY), allowZ: false, safe: true);
                if (possibleDirectionsToMove.Count == 0)
                {
                    break;
                }
                var directionIndex = Game.NextRandom(minValue: 0, maxValue: possibleDirectionsToMove.Count);
                Move(possibleDirectionsToMove[directionIndex], safe: true);
            }

            return true;
        }

        private bool TryAttackPlayerCharacter()
        {
            var playerCharacter = Game.PlayerCharacters.FirstOrDefault(pc =>
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
            var playerCharacter = Game.PlayerCharacters.FirstOrDefault(pc =>
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
                var couldMove = Move(direction.Value, safe: true);
                return couldMove;
            }

            return false;
        }

        public static Creature CreateMonster(CreatureVariant variant, byte x, byte y, Level level)
        {
            var monster = new Creature(variant, x, y, level);
            monster.AdjustInitialXPLevel();

            level.Actors.Add(monster);
            return monster;
        }

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

                var playerLevel = Game.PlayerCharacters.OrderByDescending(pc => pc.XPLevel).FirstOrDefault()?.XPLevel ??
                                  1;
                var relativePlayerLevel = playerLevel - XPLevel;
                if (relativePlayerLevel > 0)
                {
                    XPLevel += (byte)(relativePlayerLevel/4);
                }

                while (CreatureVariant.PreviousStage != null
                       && XPLevel < CreatureVariant.InitialLevel)
                {
                    OriginalVariant = CreatureVariant.PreviousStage.Name;
                }

                while (CreatureVariant.NextStage != null
                       && XPLevel >= CreatureVariant.NextStage.InitialLevel)
                {
                    OriginalVariant = CreatureVariant.NextStage.Name;
                }

                XPLevel = XPLevel < 1 ? (byte)1 : XPLevel;
                var upperLimit = (byte)Math.Min(3*CreatureVariant.InitialLevel/2, val2: 49);
                XPLevel = XPLevel > upperLimit ? upperLimit : XPLevel;
            }

            MaxHP = Game.Roll(XPLevel, diceSides: 8);

            var maxHP = Get<int>("MaxHP");
            if (maxHP > 0)
            {
                MaxHP = maxHP;
            }

            MaxHP = MaxHP < 1 ? 1 : MaxHP;
            HP = MaxHP;
        }
    }
}