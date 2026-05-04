#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1649 // File name should match first type name

using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;
using UnicornHack.Hubs.MessagePack;

namespace MessagePack.Resolvers
{
    public class GeneratedResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new GeneratedResolver();

        private GeneratedResolver()
        {
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            internal static readonly IMessagePackFormatter<T> Formatter = null!;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    Formatter = (IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        private static readonly Dictionary<Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new Dictionary<Type, int>(2)
            {
                { typeof((short, byte)), 0 },
                { typeof((short, byte)[]), 1 },
                { typeof((short, DirectionFlags)), 2 },
                { typeof((short, DirectionFlags)[]), 3 },
                { typeof((short, MapFeature)), 4 },
                { typeof((short, MapFeature)[]), 5 },
                { typeof(DirectionFlags[]), 7 },
                { typeof(MapFeature[]), 8 },
                { typeof(Dictionary<int, List<object>>), 9 },
                { typeof(Dictionary<string, int>), 10 },
                { typeof(List<(short, byte)>), 13 },
                { typeof(List<(short, DirectionFlags)>), 14 },
                { typeof(List<(short, MapFeature)>), 15 },
                { typeof(object), 16 },
                { typeof(PlayerChange), 17 },
                { typeof(LevelChange), 18 },
                { typeof(ActorChange), 19 },
                { typeof(ItemChange), 20 },
                { typeof(ConnectionChange), 21 },
                { typeof(RaceChange), 22 },
                { typeof(AbilityChange), 23 },
                { typeof(LogEntryChange), 24 },
                { typeof(TurnChangeSet), 25 },
                { typeof(Dictionary<int, ActorChange>), 26 },
                { typeof(Dictionary<int, ItemChange>), 27 },
                { typeof(Dictionary<int, ConnectionChange>), 28 },
                { typeof(Dictionary<int, RaceChange>), 29 },
                { typeof(Dictionary<int, AbilityChange>), 30 },
                { typeof(Dictionary<int, LogEntryChange>), 31 },
                { typeof(List<TurnChangeSet>), 32 },
                { typeof(LevelMap), 33 },
                { typeof(LevelMapChanges), 34 },
                { typeof(AttackSummary), 35 },
                { typeof(ActorActionChange), 36 }
            };
        }

        internal static object? GetFormatter(Type t)
        {
            if (!lookup.TryGetValue(t, out var key))
            {
                return null;
            }

            switch (key)
            {
                case 0:
                    return new ValueTupleFormatter<short, byte>();
                case 1:
                    return new ArrayFormatter<(short, byte)>();
                case 2:
                    return new ValueTupleFormatter<short, DirectionFlags>();
                case 3:
                    return new ArrayFormatter<(short, DirectionFlags)>();
                case 4:
                    return new ValueTupleFormatter<short, MapFeature>();
                case 5:
                    return new ArrayFormatter<(short, MapFeature)>();
                case 7:
                    return new ArrayFormatter<DirectionFlags>();
                case 8:
                    return new ArrayFormatter<MapFeature>();
                case 9:
                    return new DictionaryFormatter<int, List<object>>();
                case 10:
                    return new DictionaryFormatter<string, int>();
                case 13:
                    return new ListFormatter<(short, byte)>();
                case 14:
                    return new ListFormatter<(short, DirectionFlags)>();
                case 15:
                    return new ListFormatter<(short, MapFeature)>();
                case 16:
                    return new CastingFormatter();
                case 17:
                    return PlayerChangeFormatter.Instance;
                case 18:
                    return LevelChangeFormatter.Instance;
                case 19:
                    return ActorChangeFormatter.Instance;
                case 20:
                    return ItemChangeFormatter.Instance;
                case 21:
                    return ConnectionChangeFormatter.Instance;
                case 22:
                    return RaceChangeFormatter.Instance;
                case 23:
                    return AbilityChangeFormatter.Instance;
                case 24:
                    return LogEntryChangeFormatter.Instance;
                case 25:
                    return TurnChangeSetFormatter.Instance;
                case 26:
                    return new DictionaryFormatter<int, ActorChange>();
                case 27:
                    return new DictionaryFormatter<int, ItemChange>();
                case 28:
                    return new DictionaryFormatter<int, ConnectionChange>();
                case 29:
                    return new DictionaryFormatter<int, RaceChange>();
                case 30:
                    return new DictionaryFormatter<int, AbilityChange>();
                case 31:
                    return new DictionaryFormatter<int, LogEntryChange>();
                case 32:
                    return new ListFormatter<TurnChangeSet>();
                case 33:
                    return LevelMapFormatter.Instance;
                case 34:
                    return LevelMapChangesFormatter.Instance;
                case 35:
                    return AttackSummaryFormatter.Instance;
                case 36:
                    return NextActionChangeFormatter.Instance;
                default:
                    return null;
            }
        }
    }
}
