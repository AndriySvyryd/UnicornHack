using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using UnicornHack.Services;
using UnicornHack.Utils;

namespace UnicornHack.Models.GameState
{
    public class Game
    {
        public virtual int Id { get; private set; }
        public virtual int CurrentTurn { get; set; }
        public virtual int RandomSeed { get; set; }
        public virtual Actor ActingActor { get; set; }
        public virtual IList<Actor> Actors { get; set; } = new List<Actor>();
        public virtual IList<Item> Items { get; set; } = new List<Item>();
        public virtual IList<Level> Levels { get; set; } = new List<Level>();
        public virtual IList<Stairs> Stairs { get; set; } = new List<Stairs>();

        [NotMapped]
        public virtual GameServices Services { get; set; }

        [NotMapped]
        public virtual IEnumerable<PlayerCharacter> PlayerCharacters => Actors.OfType<PlayerCharacter>();

        public virtual Actor Turn()
        {
            var playerCharacters = Actors.OfType<PlayerCharacter>().ToList();
            while (playerCharacters.Any(pc => pc.IsAlive))
            {
                foreach (var level in playerCharacters.OrderBy(pc => pc.Id).Select(pc => pc.Level).Distinct().ToList())
                {
                    var actingActor = level.Turn();
                    if (actingActor != null)
                    {
                        ActingActor = actingActor;
                        return actingActor;
                    }
                }

                CurrentTurn++;
            }

            return null;
        }

        public virtual int NextRandom(int maxValue)
            => GetRandom().Next(maxValue);

        public virtual int NextRandom(int minValue, int maxValue)
        {
            var random = GetRandom();
            var result = random.Next(minValue, maxValue);
            RandomSeed = random.GetSeed();
            return result;
        }

        public virtual int Roll(int diceCount, int diceSides)
        {
            var result = 0;
            var random = GetRandom();
            for (var i = 0; i < diceCount; i++)
            {
                result += random.Next(lowerBound: 0, maxValue: diceSides) + 1;
            }

            RandomSeed = random.GetSeed();
            return result;
        }

        private SimpleRandom _random;

        private SimpleRandom GetRandom()
        {
            return _random ?? (_random = new SimpleRandom(RandomSeed));
        }
    }
}