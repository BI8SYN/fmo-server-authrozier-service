using Sas.Logging;

namespace Sas.Auth;

/// <summary>
/// Historical shipped default contents of <c>builtin/roles/*.json</c>, loaded
/// from the embedded <c>builtin/roles/history/*.json</c> resources
/// (<c>acl.history.&lt;role&gt;.&lt;label&gt;.json</c>).
///
/// On startup a role file is only upgraded when its canonical JSON hash matches
/// one of the defaults below. Any other content is treated as a local custom
/// file and left untouched.
///
/// Maintenance: the history directory is append-only. Before changing a
/// builtin role file, copy its current content into <c>history/</c> under a new
/// label and bump the file's <c>_version</c>. Never edit or delete an existing
/// history file - older deployments may still be running it.
/// </summary>
internal static class BuiltinRoleHistory
{
    private const string ResourcePrefix = "acl.history.";
    private const string JsonExtension = ".json";

    private static readonly Dictionary<string, HashSet<string>> Hashes = BuildHashes();

    public static bool IsDefault(string fileName, string hash) =>
        Hashes.TryGetValue(fileName, out var set) && set.Contains(hash);

    private static Dictionary<string, HashSet<string>> BuildHashes()
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var assembly = typeof(BuiltinRoleHistory).Assembly;

        foreach (var name in assembly.GetManifestResourceNames()
                     .Where(n => n.StartsWith(ResourcePrefix, StringComparison.Ordinal)))
        {
            var stem = name[ResourcePrefix.Length..];

            if (!stem.EndsWith(JsonExtension, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Error($"ACL roles: history resource {name} is not a .json file, ignoring");
                continue;
            }

            stem = stem[..^JsonExtension.Length];

            var separator = stem.IndexOf('.');
            if (separator <= 0 || separator == stem.Length - 1)
            {
                Logger.Error($"ACL roles: history resource {name} must be named <role>.<label>.json, ignoring");
                continue;
            }

            var fileName = stem[..separator] + JsonExtension;

            try
            {
                using var stream = assembly.GetManifestResourceStream(name);
                if (stream == null)
                    continue;

                using var reader = new StreamReader(stream);
                var hash = RoleJson.Hash(reader.ReadToEnd());

                if (hash == null)
                {
                    Logger.Error($"ACL roles: history resource {name} is not valid JSON, ignoring");
                    continue;
                }

                if (!result.TryGetValue(fileName, out var set))
                    result[fileName] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                set.Add(hash);
            }
            catch (Exception ex)
            {
                Logger.Error($"ACL roles: failed to load history resource {name}: {ex.Message}");
            }
        }

        return result;
    }
}
