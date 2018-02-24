using System.Diagnostics;
using System.Linq;
using UnicornHack.Abilities;

namespace UnicornHack
{
    public class Creature : Actor
    {
        public virtual byte DifficultyLevel { get; set; }
        public virtual int XP { get; set; }
        public MonsterBehavior Behavior { get; set; }
        public ActorNoiseType Noise { get; set; }
        public string CorpseVariantName { get; set; }
        public string PreviousStageName { get; set; }
        public string NextStageName { get; set; }

        public Creature()
        {
        }

        public Creature(Level level, byte x, byte y) : base(level, x, y)
        {
        }

        public override bool Act()
        {
            Debug.Assert(IsAlive);

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

            var possibleDirectionsToMove = Level.GetPossibleMovementDirections(LevelCell, safe: true);
            if (possibleDirectionsToMove.Count == 0)
            {
                NextActionTick += DefaultActionDelay;
                return true;
            }

            if (possibleDirectionsToMove.Contains(Heading) && Game.Random.NextBool())
            {
                return Move(Heading);
            }

            var directionIndex = Game.Random.Next(minValue: 0, maxValue: possibleDirectionsToMove.Count);
            return Move(possibleDirectionsToMove[directionIndex]);
        }

        private bool TryAttackPlayerCharacter()
        {
            var playerCharacter = Level.Players.FirstOrDefault(pc => LevelCell.DistanceTo(pc.LevelCell) <= 1 && pc.IsAlive);
            if (playerCharacter == null)
            {
                return false;
            }

            return Attack(playerCharacter);
        }

        private bool Attack(Actor victim, bool pretend = false)
        {
            var ability = Abilities.FirstOrDefault(a => a.Activation == AbilityActivation.OnTarget && a.IsUsable);
            if (ability == null)
            {
                return false;
            }

            if (pretend)
            {
                return true;
            }

            using (var context = new AbilityActivationContext
            {
                Activator = this,
                TargetEntity = victim
            })
            {
                return ability.Activate(context);
            }
        }

        private bool TryMoveToPlayerCharacter()
        {
            var playerCharacter = Level.Players.FirstOrDefault(pc => LevelCell.DistanceTo(pc.LevelCell) <= 16 && pc.IsAlive);
            if (playerCharacter == null)
            {
                return false;
            }

            // TODO: Check memory and other senses
            if (LevelCell.DistanceTo(playerCharacter.LevelCell) > 3
                && GetVisibility(playerCharacter.LevelCell) == 0)
            {
                return false;
            }

            var direction = Level.GetFirstStepFromShortestPath(this, playerCharacter);
            if (direction != null && Move(direction.Value, pretend: true))
            {
                return Move(direction.Value);
            }

            return false;
        }

        public override bool ChangeCurrentHP(int hp)
        {
            var wasAlive = IsAlive;
            var level = Level;
            var isAlive = base.ChangeCurrentHP(hp);
            if (wasAlive && !isAlive)
            {
                // TODO: Track the last interacted player
                var playerCharacter = level.Players.Where(pc => pc.IsAlive).OrderBy(pc => LevelCell.DistanceTo(pc.LevelCell))
                    .FirstOrDefault();

                playerCharacter?.AddXP(XP + DifficultyLevel * 100);
            }

            return isAlive;
        }

        protected override void Die()
        {
            DropGold(Gold);
            foreach (var item in Inventory.ToList())
            {
                Drop(item);
            }
            base.Die();
            Level.Actors.Remove(this);
            Level = null;
            RemoveReference();
        }
    }
}