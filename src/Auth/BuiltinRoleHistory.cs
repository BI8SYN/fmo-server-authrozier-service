namespace Sas.Auth;

/// <summary>
/// Historical shipped default contents of <c>builtin/roles/*.json</c>.
///
/// On startup a role file is only upgraded when its canonical JSON hash matches
/// one of the defaults below. Any other content is treated as a local custom
/// file and left untouched.
///
/// Maintenance: every time a builtin role file changes and its <c>_version</c>
/// is incremented, append the previous default content to the matching list.
/// Do not edit existing entries - older deployments may still be running them.
/// </summary>
internal static class BuiltinRoleHistory
{
    private static readonly Dictionary<string, string[]> Defaults =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["user.json"] = [UserV1],
            ["admin.json"] = [AdminV1],
            ["super.json"] = [SuperV1]
        };

    private static readonly Dictionary<string, HashSet<string>> Hashes = BuildHashes();

    public static bool IsDefault(string fileName, string hash) =>
        Hashes.TryGetValue(fileName, out var set) && set.Contains(hash);

    private static Dictionary<string, HashSet<string>> BuildHashes()
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (fileName, contents) in Defaults)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var content in contents)
            {
                var hash = RoleJson.Hash(content);
                if (hash != null)
                    set.Add(hash);
            }

            result[fileName] = set;
        }

        return result;
    }

    private const string UserV1 = """
        {
          "role": "user",
          "all": ["FMO/RAW", "FMO/TELE", "FMO/USER_RIG"],
          "pub": [
            "FMO/QSO/+",
            "FMO/LATE/UID_V1/{uid}"
          ],
          "sub": [
            "FMO/QSO/UID/{uid}",
            "FMO/LATE/UID_V1/{uid}",
            "FMO/PROFILE",
            "FMO/MUTELIST",
            "FMO/REMOTE_CONTROL",
            "FMO/SERVER_INFO/#"
          ]
        }
        """;

    private const string AdminV1 = """
        {
          "role": "admin",
          "all": ["FMO/RAW", "FMO/TELE", "FMO/USER_RIG"],
          "pub": [
            "FMO/QSO/+",
            "FMO/LATE/UID_V1/{uid}",
            "FMO/MUTELIST"
          ],
          "sub": [
            "FMO/QSO/UID/{uid}",
            "FMO/LATE/UID_V1/{uid}",
            "FMO/PROFILE",
            "FMO/REMOTE_CONTROL",
            "FMO/SERVER_INFO/#"
          ]
        }
        """;

    private const string SuperV1 = """
        {
          "role": "super",
          "all": ["FMO/RAW", "FMO/TELE", "FMO/USER_RIG"],
          "pub": [
            "FMO/QSO/+",
            "FMO/LATE/UID_V1/{uid}",
            "FMO/PROFILE",
            "FMO/MUTELIST",
            "FMO/REMOTE_CONTROL",
            "FMO/SERVER_INFO/#"
          ],
          "sub": [
            "FMO/QSO/UID/{uid}",
            "FMO/LATE/UID_V1/{uid}"
          ]
        }
        """;
}
