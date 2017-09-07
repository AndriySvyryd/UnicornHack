using System;
using System.Linq;
using UnicornHack.Data;
using UnicornHack.Services;

// ReSharper disable RedundantAssignment
namespace UnicornHack.Models.GameHubModels
{
    public class CompactActor
    {
        public object[] Properties { get; set; }
        private const int ActorPropertyCount = 6;

        public static CompactActor Serialize(Actor actor, GameDbContext context, GameServices services)
        {
            switch (actor)
            {
                case Player player:
                    return Serialize(player, new object[ActorPropertyCount + 10], context, services);
                default:
                    return Serialize(actor, new object[ActorPropertyCount], context, services);
            }
        }

        private static CompactActor Serialize(Actor actor, object[] properties, GameDbContext context, GameServices services)
        {
            var i = 0;
            properties[i++] = actor.Id;
            properties[i++] = actor.BaseName;
            properties[i++] = actor.Name;
            properties[i++] = actor.LevelX;
            properties[i++] = actor.LevelY;
            properties[i++] = (byte)actor.Heading;

            return new CompactActor
            {
                Properties = properties
            };
        }

        private static CompactActor Serialize(Player player, object[] properties, GameDbContext context, GameServices services)
        {
            var i = ActorPropertyCount;
            properties[i++] = player.Properties.Where(p => !(p.Value is bool value) || value)
                .Select(p => CompactProperty.Serialize(p, context, services)).ToArray();
            properties[i++] = player.Inventory.OrderBy(t => t.Name).Select(t => CompactItem.Serialize(t, context, services)).ToArray();
            properties[i++] = player.Abilities.Where(a => a.IsUsable && a.Activation == AbilityActivation.OnTarget)
                .Select(a => CompactAbility.Serialize(a, context, services)).ToArray();
            properties[i++] = player.NextActionTick;
            properties[i++] = player.XP;
            properties[i++] = player.NextLevelXP;
            properties[i++] = player.XPLevel;
            properties[i++] = player.Gold;
            properties[i++] = player.Log.OrderBy(e => e.Id).Skip(Math.Max(0, player.Log.Count - 10))
                .Select(e => CompactLogEntry.Serialize(e, context, services)).ToArray();
            properties[i++] = player.Races.OrderBy(r => r.Name).Select(r => CompactPlayerRace.Serialize(r, context, services)).ToArray();

            return Serialize((Actor)player, properties, context, services);
        }
    }
}