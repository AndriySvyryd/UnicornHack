using System.Collections;
using System.Globalization;
using System.Text;

namespace UnicornHack.DiagnosticClient.Tools;

/// <summary>
///     Renders the loosely-typed <c>List&lt;object?&gt;</c> payloads returned by <c>GameHub.QueryGame</c>
///     into a human-readable indented text form. The payloads are nested arrays and dictionaries
///     (see <c>GameQueryProcessor.Serialize*</c> on the server) keyed by integer indices; this renderer
///     keeps the shape verbatim instead of trying to map back to DTO names.
/// </summary>
public static class DialogPayloadFormatter
{
    private const int MaxDepth = 8;

    public static string Render(IReadOnlyList<object?>? payload)
    {
        if (payload is null)
        {
            return "(empty)";
        }
        var sb = new StringBuilder();
        sb.Append("elements=").Append(payload.Count).Append('\n');
        for (var i = 0; i < payload.Count; i++)
        {
            sb.Append('[').Append(i).Append("] ");
            Append(sb, payload[i], indent: 2, depth: 0);
            sb.Append('\n');
        }
        return sb.ToString();
    }

    private static void Append(StringBuilder sb, object? value, int indent, int depth)
    {
        if (value is null)
        {
            sb.Append("null");
            return;
        }

        if (depth >= MaxDepth)
        {
            sb.Append("…");
            return;
        }

        switch (value)
        {
            case string s:
                sb.Append('"').Append(s).Append('"');
                return;
            case BitArray bits:
                sb.Append("bits[").Append(bits.Length).Append("] ");
                for (var i = 0; i < bits.Length; i++)
                {
                    sb.Append(bits[i] ? '1' : '0');
                }
                return;
            case IDictionary dict:
                AppendDictionary(sb, dict, indent, depth);
                return;
            case IEnumerable enumerable when value is not string:
                AppendEnumerable(sb, enumerable, indent, depth);
                return;
            case IFormattable formattable:
                sb.Append(formattable.ToString(null, CultureInfo.InvariantCulture));
                return;
            default:
                sb.Append(value);
                return;
        }
    }

    private static void AppendEnumerable(StringBuilder sb, IEnumerable enumerable, int indent, int depth)
    {
        var items = new List<object?>();
        foreach (var item in enumerable)
        {
            items.Add(item);
        }
        if (items.Count == 0)
        {
            sb.Append("[]");
            return;
        }
        if (IsScalar(items))
        {
            sb.Append('[');
            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                Append(sb, items[i], indent, depth + 1);
            }
            sb.Append(']');
            return;
        }
        sb.Append('[').Append(items.Count).Append(']');
        var childIndent = indent + 2;
        for (var i = 0; i < items.Count; i++)
        {
            sb.Append('\n').Append(' ', indent).Append(i).Append(": ");
            Append(sb, items[i], childIndent, depth + 1);
        }
    }

    private static void AppendDictionary(StringBuilder sb, IDictionary dict, int indent, int depth)
    {
        if (dict.Count == 0)
        {
            sb.Append("{}");
            return;
        }
        sb.Append('{').Append(dict.Count).Append('}');
        var childIndent = indent + 2;
        foreach (DictionaryEntry entry in dict)
        {
            sb.Append('\n').Append(' ', indent);
            Append(sb, entry.Key, childIndent, depth + 1);
            sb.Append(": ");
            Append(sb, entry.Value, childIndent, depth + 1);
        }
    }

    private static bool IsScalar(List<object?> items)
    {
        foreach (var item in items)
        {
            if (item is null or string)
            {
                continue;
            }
            if (item is IEnumerable and not string)
            {
                return false;
            }
        }
        return true;
    }
}
