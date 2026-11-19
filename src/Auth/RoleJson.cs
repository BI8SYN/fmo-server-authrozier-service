using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Sas.Auth;

/// <summary>
/// Canonical JSON representation used to compare role files. Whitespace, line
/// endings, property order and the <c>_version</c> field are ignored so that a
/// file saved by an editor is still recognised as a shipped default.
/// </summary>
internal static class RoleJson
{
    public static string? Canonicalize(string text)
    {
        try
        {
            var node = JsonNode.Parse(text);
            if (node == null)
                return null;

            var sb = new StringBuilder();
            Write(node, sb);
            return sb.ToString();
        }
        catch
        {
            return null;
        }
    }

    public static string? Hash(string text)
    {
        var canonical = Canonicalize(text);
        if (canonical == null)
            return null;

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static void Write(JsonNode? node, StringBuilder sb)
    {
        switch (node)
        {
            case JsonObject obj:
            {
                sb.Append('{');
                var first = true;
                foreach (var (key, value) in obj.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                {
                    if (key == "_version")
                        continue;

                    if (!first)
                        sb.Append(',');
                    first = false;

                    sb.Append(JsonSerializer.Serialize(key));
                    sb.Append(':');
                    Write(value, sb);
                }

                sb.Append('}');
                break;
            }
            case JsonArray array:
            {
                sb.Append('[');
                for (var i = 0; i < array.Count; i++)
                {
                    if (i > 0)
                        sb.Append(',');
                    Write(array[i], sb);
                }

                sb.Append(']');
                break;
            }
            case null:
                sb.Append("null");
                break;
            default:
                sb.Append(node.ToJsonString());
                break;
        }
    }
}
