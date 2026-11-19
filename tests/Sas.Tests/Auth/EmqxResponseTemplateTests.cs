using System.Text.Json;
using Sas.Auth;

namespace Sas.Tests.Auth;

public sealed class EmqxResponseTemplateTests
{
    private readonly EmqxResponseTemplate _template = new();

    private static JsonElement Serialize(object value) =>
        JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement.Clone();

    private static AuthResult AllowResult(bool isSuperuser = false) => new()
    {
        Callsign = "BG6VMZ",
        Uid = 50001,
        Role = isSuperuser ? "super" : "user",
        IsSuperuser = isSuperuser,
        IssuedAt = 1_700_000_000,
        ExpireAt = 1_700_014_400,
        AllTopics = ["FMO/RAW", "FMO/TELE"],
        PubTopics = ["FMO/QSO/+", "FMO/LATE/UID_V1/50001"],
        SubTopics = ["FMO/QSO/UID/50001"],
        DenyRules =
        [
            new AclDenyRule { Action = "all", Topic = "FMO/SAFEPROBE/#" },
            new AclDenyRule { Action = "publish", Topic = "FMO/PROFILE" }
        ]
    };

    [Fact]
    public void BuildAllow_ProducesEmqxShape()
    {
        var json = Serialize(_template.BuildAllow(AllowResult()));

        Assert.Equal("allow", json.GetProperty("result").GetString());
        Assert.False(json.GetProperty("is_superuser").GetBoolean());
        Assert.Equal(1_700_014_400, json.GetProperty("expire_at").GetInt64());

        var attrs = json.GetProperty("client_attrs");
        Assert.Equal("BG6VMZ", attrs.GetProperty("callsign").GetString());
        Assert.Equal("50001", attrs.GetProperty("uid").GetString());
    }

    [Fact]
    public void BuildAllow_EmitsAllowRulesBeforeDenyRules()
    {
        var acl = Serialize(_template.BuildAllow(AllowResult())).GetProperty("acl");

        Assert.Equal(7, acl.GetArrayLength());

        var rules = acl.EnumerateArray()
            .Select(e => (
                e.GetProperty("permission").GetString(),
                e.GetProperty("action").GetString(),
                e.GetProperty("topic").GetString()))
            .ToArray();

        Assert.Equal(
        [
            ("allow", "all", "FMO/RAW"),
            ("allow", "all", "FMO/TELE"),
            ("allow", "publish", "FMO/QSO/+"),
            ("allow", "publish", "FMO/LATE/UID_V1/50001"),
            ("allow", "subscribe", "FMO/QSO/UID/50001"),
            ("deny", "all", "FMO/SAFEPROBE/#"),
            ("deny", "publish", "FMO/PROFILE")
        ], rules);
    }

    [Fact]
    public void BuildAllow_SetsSuperuserFlag()
    {
        var json = Serialize(_template.BuildAllow(AllowResult(isSuperuser: true)));

        Assert.True(json.GetProperty("is_superuser").GetBoolean());
    }

    [Fact]
    public void BuildAllow_WithNoTopics_EmitsEmptyAclArray()
    {
        var result = new AuthResult
        {
            Callsign = "BG6VMZ",
            Uid = 1,
            AllTopics = [],
            PubTopics = [],
            SubTopics = []
        };

        var acl = Serialize(_template.BuildAllow(result)).GetProperty("acl");

        Assert.Equal(JsonValueKind.Array, acl.ValueKind);
        Assert.Equal(0, acl.GetArrayLength());
    }

    [Fact]
    public void BuildAllow_WithOnlyDenyRules_EmitsDenyEntries()
    {
        var result = new AuthResult
        {
            Callsign = "BG6VMZ",
            Uid = 50001,
            DenyRules =
            [
                new AclDenyRule { Action = "all", Topic = "FMO/SAFEPROBE/#" }
            ]
        };

        var acl = Serialize(_template.BuildAllow(result)).GetProperty("acl");

        Assert.Equal(1, acl.GetArrayLength());
        var entry = acl[0];
        Assert.Equal("deny", entry.GetProperty("permission").GetString());
        Assert.Equal("all", entry.GetProperty("action").GetString());
        Assert.Equal("FMO/SAFEPROBE/#", entry.GetProperty("topic").GetString());
    }

    [Fact]
    public void BuildDeny_SerialisesToSingleDenyField()
    {
        var raw = JsonSerializer.Serialize(_template.BuildDeny());

        Assert.Equal("""{"result":"deny"}""", raw);
    }
}
