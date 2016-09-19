using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Models.GameDefinitions;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameState
{
    public class Monster : Actor
    {
        public Monster()
        {
        }

        public Monster(MonsterVariant variant, byte x, byte y, Level level)
            : base(variant, x, y, level)
        {
            XPLevel = variant.InitialLevel;
        }

        public new virtual MonsterVariant Variant => (MonsterVariant)base.Variant;

        public override byte MovementRate => Variant.MovementRate;

        public override IReadOnlyList<Attack> Attacks => Variant.Attacks;

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
                    new Point(LevelX, LevelY), allowZ: false, safe:true);
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

        public static Monster CreateMonster(MonsterVariant variant, byte x, byte y, Level level)
        {
            var monster = new Monster(variant, x, y, level);
            monster.AdjustInitialXPLevel();

            level.Actors.Add(monster);
            return monster;
        }

        public virtual void AdjustInitialXPLevel()
        {
            if (Variant == MonsterVariant.Rodney)
            {
                // TODO: Adjust based on number of deaths
            }
            else if (XPLevel < 50)
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

                while (Variant.PreviousStage != null
                       && XPLevel < Variant.InitialLevel)
                {
                    OriginalVariant = Variant.PreviousStage.Name;
                }

                while (Variant.NextStage != null
                       && XPLevel >= Variant.NextStage.InitialLevel)
                {
                    OriginalVariant = Variant.NextStage.Name;
                }

                XPLevel = XPLevel < 1 ? (byte)1 : XPLevel;
                var upperLimit = (byte)Math.Min(3*Variant.InitialLevel/2, val2: 49);
                XPLevel = XPLevel > upperLimit ? upperLimit : XPLevel;
            }

            MaxHP = Game.Roll(XPLevel, diceSides: 8);

            var properties = Get<int>(ValuedActorPropertyType.MaxHP);
            if (properties.Count > 0)
            {
                if (properties[index: 0].Relative)
                {
                    foreach (var property in properties)
                    {
                        Debug.Assert(property.Relative);
                        if (property.Granted)
                        {
                            MaxHP += property.Value;
                        }
                        else
                        {
                            MaxHP -= property.Value;
                        }
                    }
                }
                else
                {
                    Debug.Assert(properties.Count == 1);
                    MaxHP = properties[index: 0].Value;
                }
            }

            MaxHP = MaxHP < 1 ? 1 : MaxHP;
            HP = MaxHP;
        }
    }
}