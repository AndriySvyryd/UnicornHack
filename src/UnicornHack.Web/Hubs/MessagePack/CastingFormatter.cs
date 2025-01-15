using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Web;

namespace UnicornHack.Hubs.MessagePack
{
    public sealed class CastingFormatter : IMessagePackFormatter<object?>
    {
        private delegate void SerializeMethod(object formatter, ref MessagePackWriter writer, object value, MessagePackSerializerOptions options);

        public static readonly IMessagePackFormatter<object?> Instance = new CastingFormatter();

        private static readonly ConcurrentDictionary<Type, (object, SerializeMethod)> Serializers = new();

        private static readonly HashSet<Type> BuiltinTypes = new HashSet<Type>
        {
            typeof(Boolean),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(string),
            typeof(byte[]),

            typeof(Boolean?),
            typeof(SByte?),
            typeof(Byte?),
            typeof(Int16?),
            typeof(UInt16?),
            typeof(Int32?),
            typeof(UInt32?),
            typeof(Int64?),
            typeof(UInt64?),
            typeof(Single?),
            typeof(Double?),
        };

        static CastingFormatter()
        {
            Serializers.TryAdd(typeof(object),
                (Instance, (object p1, ref MessagePackWriter p2, object p3, MessagePackSerializerOptions p4) => { }));
        }

        public void Serialize(ref MessagePackWriter writer, object? value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            var type = value.GetType();
            var ti = type.GetTypeInfo();
            if (ti.IsAnonymous() || BuiltinTypes.Contains(type))
            {
                DynamicObjectTypeFallbackFormatter.Instance.Serialize(ref writer, value, options);
                return;
            }

            if (!Serializers.TryGetValue(type, out var serializePair))
            {
                lock (Serializers)
                {
                    if (!Serializers.TryGetValue(type, out serializePair))
                    {
                        var formatter = options.Resolver.GetFormatterDynamic(type);
                        if (formatter == null)
                        {
                            throw new FormatterNotRegisteredException(
                                type.FullName + " is not registered in resolver: " + options.Resolver.GetType());
                        }

                        var formatterType = typeof(IMessagePackFormatter<>).MakeGenericType(type);
                        var param0 = Expression.Parameter(typeof(object), "formatter");
                        var param1 = Expression.Parameter(typeof(MessagePackWriter).MakeByRefType(), "writer");
                        var param2 = Expression.Parameter(typeof(object), "value");
                        var param3 = Expression.Parameter(typeof(MessagePackSerializerOptions), "options");

                        var serializeMethodInfo = formatterType.GetRuntimeMethod("Serialize", [typeof(MessagePackWriter).MakeByRefType(), type, typeof(MessagePackSerializerOptions)])!;

                        var body = Expression.Call(
                            Expression.Convert(param0, formatterType),
                            serializeMethodInfo,
                            param1,
                            ti.IsValueType ? Expression.Unbox(param2, type) : Expression.Convert(param2, type),
                            param3);

                        var serializeMethod = Expression.Lambda<SerializeMethod>(body, param0, param1, param2, param3).Compile();

                        serializePair = (formatter, serializeMethod);
                        Serializers.TryAdd(type, serializePair);
                    }
                }
            }

            {
                var (formatter, serializeMethod) = serializePair;
                serializeMethod!(formatter, ref writer, value, options);
            }
        }

        public object? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            return DynamicObjectTypeFallbackFormatter.Instance.Deserialize(ref reader, options);
        }
    }
}
