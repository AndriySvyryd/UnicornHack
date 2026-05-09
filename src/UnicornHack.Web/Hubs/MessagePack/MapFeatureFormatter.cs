using MessagePack;
using MessagePack.Formatters;

namespace UnicornHack.Hubs.MessagePack;

/// <summary>
///     Serialises <see cref="MapFeature" /> as its underlying <see cref="byte" /> value.
///     Required because the <see cref="MessagePack.Resolvers.CompositeResolver" /> wired up
///     in <c>Program.cs</c> only chains <c>BuiltinResolver</c> + <c>GeneratedResolver</c>;
///     neither knows how to format an arbitrary user enum on its own. <see cref="ArrayFormatter{T}" />
///     and <see cref="ValueTupleFormatter{T1,T2}" /> defer to the resolver for the element type,
///     so a missing scalar formatter cascades into "FormatterNotRegisteredException".
/// </summary>
public sealed class MapFeatureFormatter : IMessagePackFormatter<MapFeature>
{
    public static readonly MapFeatureFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, MapFeature value, MessagePackSerializerOptions options)
        => writer.Write((byte)value);

    public MapFeature Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        => (MapFeature)reader.ReadByte();
}
