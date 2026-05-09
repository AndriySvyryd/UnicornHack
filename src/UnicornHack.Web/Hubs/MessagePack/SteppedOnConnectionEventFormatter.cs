using MessagePack;
using MessagePack.Formatters;
using UnicornHack.Hubs.ChangeTracking;

namespace UnicornHack.Hubs.MessagePack;

/// <summary>
///     Serializes a <see cref="SteppedOnConnectionEvent" /> as a fixed-length array
///     <c>[Type, Scope, ConnectionId]</c>. The first slot is the
///     <see cref="TurnEventType" /> discriminator so a typeless client (which receives
///     events as <see cref="System.Collections.IList" /> of <see cref="object" />) can
///     dispatch on event kind by reading index 0. See
///     <c>NetworkCommunicationManager.ApplyEvents</c> on the Unity side.
/// </summary>
public sealed class SteppedOnConnectionEventFormatter : IMessagePackFormatter<SteppedOnConnectionEvent?>
{
    public static readonly SteppedOnConnectionEventFormatter Instance = new();

    public void Serialize(
        ref MessagePackWriter writer, SteppedOnConnectionEvent? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(3);
        writer.Write((int)value.Type);
        writer.Write((int)value.Scope);
        writer.Write(value.ConnectionId);
    }

    public SteppedOnConnectionEvent? Deserialize(
        ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        var value = new SteppedOnConnectionEvent();
        var count = reader.ReadArrayHeader();
        for (var i = 0; i < count; i++)
        {
            switch (i)
            {
                case 0:
                    value.Type = (TurnEventType)reader.ReadInt32();
                    break;
                case 1:
                    value.Scope = (ConnectionScope)reader.ReadInt32();
                    break;
                case 2:
                    value.ConnectionId = reader.ReadInt32();
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return value;
    }
}
