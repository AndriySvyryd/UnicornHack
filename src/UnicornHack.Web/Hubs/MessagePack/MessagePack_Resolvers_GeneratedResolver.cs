#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1649 // File name should match first type name

using MessagePack.Formatters;
using UnicornHack.Hubs;
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
                { typeof(DirectionFlags), 6 },
                { typeof(DirectionFlags[]), 7 },
                { typeof(MapFeature[]), 8 },
                { typeof(Dictionary<string, int>), 10 },
                { typeof(MapFeature), 11 },
                { typeof(List<(short, byte)>), 13 },
                { typeof(List<(short, DirectionFlags)>), 14 },
                { typeof(List<(short, MapFeature)>), 15 },
                { typeof(PlayerChange), 17 },
                { typeof(RoomChange), 18 },
                { typeof(ActorChange), 19 },
                { typeof(ItemChange), 20 },
                { typeof(ConnectionChange), 21 },
                { typeof(AbilityChange), 23 },
                { typeof(LogEntryChange), 24 },
                { typeof(TurnChangeSet), 25 },
                { typeof(Dictionary<int, ActorChange>), 26 },
                { typeof(Dictionary<int, ItemChange>), 27 },
                { typeof(Dictionary<int, ConnectionChange>), 28 },
                { typeof(Dictionary<int, AbilityChange>), 30 },
                { typeof(Dictionary<int, LogEntryChange>), 31 },
                { typeof(List<TurnChangeSet>), 32 },
                { typeof(RoomMap), 33 },
                { typeof(RoomMapChanges), 34 },
                { typeof(ActorActionChange), 36 },
                { typeof(LevelMap), 37 },
                { typeof(RoomEntry), 38 },
                { typeof(List<RoomEntry>), 39 },
                { typeof(List<int>), 40 },
                { typeof(TurnEvent), 41 },
                { typeof(EffectChange), 42 },
                { typeof(Dictionary<int, EffectChange>), 43 },
                { typeof(DialogData), 44 },
                { typeof(PlayerInventory), 45 },
                { typeof(UnicornHack.Hubs.Item), 46 },
                { typeof(EquippableSlot), 47 },
                { typeof(PlayerAdaptations), 48 },
                { typeof(PlayerSkills), 49 },
                { typeof(ActorAttributes), 50 },
                { typeof(AbilityAttributes), 51 },
                { typeof(EffectAttributes), 52 },
                { typeof(SubAbilityAttributes), 53 },
                { typeof(ItemAttributes), 54 },
                { typeof(PostGameStatistics), 55 },
                { typeof(List<TurnEvent>), 56 },
                { typeof(Dictionary<int, UnicornHack.Hubs.Item>), 57 },
                { typeof(Dictionary<int, EquippableSlot>), 58 },
                { typeof(Dictionary<int, EffectAttributes>), 59 },
                { typeof(List<SubAbilityAttributes>), 60 },
                { typeof(List<AbilityAttributes>), 61 },
                { typeof(ConditionalGrantAttributes), 62 },
                { typeof(List<ConditionalGrantAttributes>), 63 }
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
                case 6:
                    return DirectionFlagsFormatter.Instance;
                case 7:
                    return new ArrayFormatter<DirectionFlags>();
                case 8:
                    return new ArrayFormatter<MapFeature>();
                case 10:
                    return new DictionaryFormatter<string, int>();
                case 11:
                    return MapFeatureFormatter.Instance;
                case 13:
                    return new ListFormatter<(short, byte)>();
                case 14:
                    return new ListFormatter<(short, DirectionFlags)>();
                case 15:
                    return new ListFormatter<(short, MapFeature)>();
                case 17:
                    return PlayerChangeFormatter.Instance;
                case 18:
                    return RoomChangeFormatter.Instance;
                case 19:
                    return ActorChangeFormatter.Instance;
                case 20:
                    return ItemChangeFormatter.Instance;
                case 21:
                    return ConnectionChangeFormatter.Instance;
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
                case 30:
                    return new DictionaryFormatter<int, AbilityChange>();
                case 31:
                    return new DictionaryFormatter<int, LogEntryChange>();
                case 32:
                    return new ListFormatter<TurnChangeSet>();
                case 33:
                    return RoomMapFormatter.Instance;
                case 34:
                    return RoomMapChangesFormatter.Instance;
                case 36:
                    return NextActionChangeFormatter.Instance;
                case 37:
                    return LevelMapFormatter.Instance;
                case 38:
                    return RoomEntryFormatter.Instance;
                case 39:
                    return new ListFormatter<RoomEntry>();
                case 40:
                    return new ListFormatter<int>();
                case 41:
                    return TurnEventFormatter.Instance;
                case 42:
                    return EffectChangeFormatter.Instance;
                case 43:
                    return new DictionaryFormatter<int, EffectChange>();
                case 44:
                    return DialogDataFormatter.Instance;
                case 45:
                    return PlayerInventoryFormatter.Instance;
                case 46:
                    return DialogItemFormatter.Instance;
                case 47:
                    return EquippableSlotFormatter.Instance;
                case 48:
                    return PlayerAdaptationsFormatter.Instance;
                case 49:
                    return PlayerSkillsFormatter.Instance;
                case 50:
                    return ActorAttributesFormatter.Instance;
                case 51:
                    return AbilityAttributesFormatter.Instance;
                case 52:
                    return EffectAttributesFormatter.Instance;
                case 53:
                    return SubAbilityAttributesFormatter.Instance;
                case 54:
                    return ItemAttributesFormatter.Instance;
                case 55:
                    return PostGameStatisticsFormatter.Instance;
                case 56:
                    return new ListFormatter<TurnEvent>();
                case 57:
                    return new DictionaryFormatter<int, UnicornHack.Hubs.Item>();
                case 58:
                    return new DictionaryFormatter<int, EquippableSlot>();
                case 59:
                    return new DictionaryFormatter<int, EffectAttributes>();
                case 60:
                    return new ListFormatter<SubAbilityAttributes>();
                case 61:
                    return new ListFormatter<AbilityAttributes>();
                case 62:
                    return ConditionalGrantAttributesFormatter.Instance;
                case 63:
                    return new ListFormatter<ConditionalGrantAttributes>();
                default:
                    return null;
            }
        }
    }
}
