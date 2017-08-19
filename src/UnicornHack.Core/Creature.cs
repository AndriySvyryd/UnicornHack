using System.Linq;
using UnicornHack.Generation;
using UnicornHack.Utils;

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

            var possibleDirectionsToMove = Level.GetPossibleMovementDirections(new Point(LevelX, LevelY), safe: true);
            if (possibleDirectionsToMove.Count == 0)
            {
                NextActionTick += DefaultActionDelay;
                return true;
            }

            if (possibleDirectionsToMove.Contains(Heading) && Game.Random.NextBool())
            {
                return Move(Heading, safe: true);
            }

            var directionIndex = Game.Random.Next(minValue: 0, maxValue: possibleDirectionsToMove.Count);
            return Move(possibleDirectionsToMove[directionIndex], safe: true);
        }

        private bool TryAttackPlayerCharacter()
        {
            var playerCharacter = Level.Players.FirstOrDefault(pc => Level.GridDistance(this, pc) <= 1 && pc.IsAlive);
            if (playerCharacter == null)
            {
                return false;
            }

            return Attack(playerCharacter);
        }

        private bool TryMoveToPlayerCharacter()
        {
            var playerCharacter = Level.Players.FirstOrDefault(pc => Level.GridDistance(this, pc) <= 6 && pc.IsAlive);
            if (playerCharacter == null)
            {
                return false;
            }

            var direction = Level.GetFirstStepFromShortestPath(this, playerCharacter);
            if (direction != null && Move(direction.Value, pretend: true, safe: true))
            {
                return Move(direction.Value, safe: true);
            }

            return false;
        }

        public override bool ChangeCurrentHP(int hp)
        {
            var wasAlive = IsAlive;
            var isAlive = base.ChangeCurrentHP(hp);
            if (wasAlive && !isAlive)
            {
                // TODO: Track the last interacted player
                var playerCharacter = Level.Players.Where(pc => pc.IsAlive).OrderBy(pc => Level.GridDistance(this, pc))
                    .FirstOrDefault();

                // TODO: Calculate diminishing XP
                playerCharacter?.AddXP(XP + DifficultyLevel * 10);
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
            RemoveReference();
        }
    }
}