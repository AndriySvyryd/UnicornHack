using MessagePack;
using MessagePack.Formatters;

namespace UnicornHack.Hubs.MessagePack;

/// <summary>
///     Serialises <see cref="DirectionFlags" /> as its underlying <see cref="int" /> value.
///     See <see cref="MapFeatureFormatter" /> for why a manual scalar formatter is required.
/// </summary>
public sealed class DirectionFlagsFormatter : IMessagePackFormatter<DirectionFlags>
{
    public static readonly DirectionFlagsFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, DirectionFlags value, MessagePackSerializerOptions options)
        => writer.Write((byte)value);

    public DirectionFlags Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        => (DirectionFlags)reader.ReadByte();
}
