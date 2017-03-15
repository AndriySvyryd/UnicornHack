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
        public virtual ICollection<Branch> Branches { get; set; } = new HashSet<Branch>();
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

        public virtual int NextRandom(int maxValue) => GetRandom().Next(maxValue);

        public virtual int NextRandom(int minValue, int maxValue)
        {
            var random = GetRandom();
            var result = random.Next(minValue, maxValue);
            RandomSeed = random.Seed;
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

            RandomSeed = random.Seed;
            return result;
        }

        public virtual T Pick<T>(IReadOnlyList<T> items, Func<T, float> getWeight)
        {
            if (items == null
                || items.Count == 0)
            {
                throw new InvalidOperationException();
            }

            var cumulativeWeights = new float[items.Count];
            var sum = 0f;
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var weight = getWeight(item);
                if (float.IsPositiveInfinity(weight))
                {
                    return item;
                }
                if (weight < 0)
                {
                    weight = 0;
                }

                sum += weight;
                cumulativeWeights[i] = sum;
            }

            return items[BinarySearch(cumulativeWeights, _random.Next(0, sum))];
        }

        private static int BinarySearch(float[] mynumbers, float target)
        {
            Debug.Assert(mynumbers.Length > 0);

            var first = 0;
            var last = mynumbers.Length - 1;
            var midPoint = 0;
            while (first <= last)
            {
                midPoint = (first + last) / 2;
                var midValue = mynumbers[midPoint];

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (target == midValue)
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    while (midPoint > 0 && target == mynumbers[midPoint - 1])
                    {
                        midPoint--;
                    }
                    return midPoint;
                }

                if (target > midValue)
                {
                    first = midPoint + 1;
                }

                if (target < midValue)
                {
                    last = midPoint - 1;
                }
            }

            return midPoint == mynumbers.Length ? midPoint - 1 : midPoint;
        }

        private SimpleRandom _random;

        private SimpleRandom GetRandom() => _random ?? (_random = new SimpleRandom(RandomSeed));
    }
}