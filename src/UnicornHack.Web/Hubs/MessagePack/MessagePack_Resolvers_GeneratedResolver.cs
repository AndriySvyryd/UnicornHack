#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1649 // File name should match first type name

using MessagePack.Formatters;
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
                { typeof(List<List<object>>), 11 },
                { typeof(List<object>), 12 },
                { typeof(List<(short, byte)>), 13 },
                { typeof(List<(short, DirectionFlags)>), 14 },
                { typeof(List<(short, MapFeature)>), 15 },
                { typeof(object), 16 }
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
                case 11:
                    return new ListFormatter<List<object>>();
                case 12:
                    return new ListFormatter<object>();
                case 13:
                    return new ListFormatter<(short, byte)>();
                case 14:
                    return new ListFormatter<(short, DirectionFlags)>();
                case 15:
                    return new ListFormatter<(short, MapFeature)>();
                case 16:
                    return new CastingFormatter();
                default: return null;
            }
        }
    }
}
