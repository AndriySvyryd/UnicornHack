using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation.Map;
using UnicornHack.Services;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Game
    {
        public virtual int Id { get; private set; }

        public virtual int NextPlayerTick { get; set; }
        public virtual int EventOrder { get; set; }
        public virtual int RandomSeed { get; set; }
        public virtual int WorldSeed { get; set; }
        public virtual bool NonRandomSeed { get; set; }

        public virtual int NextActorId { get; set; }
        public virtual ICollection<Actor> Actors { get; set; } = new HashSet<Actor>();
        public virtual int NextItemId { get; set; }
        public virtual ICollection<Item> Items { get; set; } = new HashSet<Item>();
        public virtual int NextLevelId { get; set; }
        public virtual ICollection<Level> Levels { get; set; } = new HashSet<Level>();
        public virtual int NextStairsId { get; set; }
        public virtual ICollection<Stairs> Stairs { get; set; } = new HashSet<Stairs>();
        public virtual int NextAbilityId { get; set; }
        public virtual ICollection<Ability> Abilities { get; set; } = new HashSet<Ability>();
        public virtual int NextEffectId { get; set; }
        public virtual ICollection<Effect> Effects { get; set; } = new HashSet<Effect>();
        public virtual ICollection<SensoryEvent> SensoryEvents { get; set; } = new HashSet<SensoryEvent>();
        public virtual GameServices Services { get; set; }
        public virtual Action<object> Delete { get; set; }
        public virtual IEnumerable<Player> Players => Actors.OfType<Player>();

        public virtual Actor Turn()
        {
            var orderedPlayers = new PriorityQueue<Player>(Players, Actor.TickComparer.Instance);
            while (orderedPlayers.Any(pc => pc.IsAlive))
            {
                var player = orderedPlayers.Peek();
                NextPlayerTick = player.NextActionTick;
                var actingActor = player.Level.Turn();
                if (actingActor != null)
                {
                    NextPlayerTick = player.NextActionTick;
                    return actingActor;
                }

                Debug.Assert(NextPlayerTick != player.NextActionTick,
                    nameof(Player.NextActionTick) + " hasn't been updated!");
                orderedPlayers.Update(0);
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