using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using UnicornHack.Effects;
using UnicornHack.Events;
using UnicornHack.Generation;
using UnicornHack.Services;
using UnicornHack.Utils;

namespace UnicornHack
{
    public class Game
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public virtual int Id { get; private set; }

        public virtual int NextPlayerTick { get; set; }
        public virtual int EventOrder { get; set; }
        public virtual int? InitialSeed { get; set; }
        public virtual SimpleRandom Random { get; set; }

        public virtual int NextEntityId { get; set; }
        public virtual ICollection<Entity> Entities { get; set; } = new HashSet<Entity>();
        public virtual ICollection<Branch> Branches { get; set; } = new HashSet<Branch>();
        public virtual ICollection<Level> Levels { get; set; } = new HashSet<Level>();
        public virtual ICollection<Room> Rooms { get; set; } = new HashSet<Room>();
        public virtual int NextConnectionId { get; set; }
        public virtual ICollection<Connection> Connections { get; set; } = new HashSet<Connection>();
        public virtual int NextAbilityDefinitionId { get; set; }
        public virtual ICollection<AbilityDefinition> AbilityDefinitions { get; set; } = new HashSet<AbilityDefinition>();
        public virtual int NextAbilityId { get; set; }
        public virtual ICollection<Ability> Abilities { get; set; } = new HashSet<Ability>();
        public virtual int NextEffectId { get; set; }
        public virtual ICollection<Effect> Effects { get; set; } = new HashSet<Effect>();
        public int NextAppliedEffectId { get; set; }
        public virtual ICollection<AppliedEffect> AppliedEffects { get; set; } = new HashSet<AppliedEffect>();
        public virtual ICollection<SensoryEvent> SensoryEvents { get; set; } = new HashSet<SensoryEvent>();
        public virtual GameServices Services { get; set; }
        public virtual IRepository Repository { get; set; }
        public virtual IEnumerable<Player> Players => Entities.OfType<Player>();

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

        public virtual (int[,], Point[]) GetPointIndex(byte width, byte height) => Services.SharedCache.GetOrCreate(
            nameof(GetPointIndex).GetHashCode() ^ (width << 8 + height), e =>
            {
                var pointToIndex = new int[width, height];
                var indexToPoint = new Point[width * height];
                var i = 0;
                for (byte y = 0; y < height; y++)
                {
                    for (byte x = 0; x < width; x++)
                    {
                        pointToIndex[x, y] = i;
                        indexToPoint[i++] = new Point(x, y);
                    }
                }

                return (pointToIndex, indexToPoint);
            });

        public virtual Branch GetBranch(string branchName) => Repository.Find<Branch>(Id, branchName);

        public virtual Level GetLevel(string branchName, byte depth) => Repository.Find<Level>(Id, branchName, depth);
    }
}