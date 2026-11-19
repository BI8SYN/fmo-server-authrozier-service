using Sas.Auth;

namespace Sas.Tests.Auth;

public sealed class RoleJsonTests
{
    [Fact]
    public void Hash_IgnoresWhitespaceAndPropertyOrder()
    {
        const string compact = """{"role":"user","all":["FMO/RAW"],"pub":[],"sub":[]}""";
        const string formatted = """
            {
              "sub": [],
              "all": [
                "FMO/RAW"
              ],
              "role": "user",
              "pub": []
            }
            """;

        Assert.Equal(RoleJson.Hash(compact), RoleJson.Hash(formatted));
    }

    [Fact]
    public void Hash_IgnoresLineEndings()
    {
        const string lf = "{\n  \"role\": \"user\",\n  \"all\": []\n}";
        const string crlf = "{\r\n  \"role\": \"user\",\r\n  \"all\": []\r\n}";

        Assert.Equal(RoleJson.Hash(lf), RoleJson.Hash(crlf));
    }

    [Fact]
    public void Hash_IgnoresVersionField()
    {
        const string v1 = """{"role":"user","_version":1,"all":["FMO/RAW"]}""";
        const string v9 = """{"role":"user","_version":9,"all":["FMO/RAW"]}""";

        Assert.Equal(RoleJson.Hash(v1), RoleJson.Hash(v9));
    }

    [Fact]
    public void Hash_IsSensitiveToArrayOrder()
    {
        const string a = """{"role":"user","all":["FMO/RAW","FMO/TELE"]}""";
        const string b = """{"role":"user","all":["FMO/TELE","FMO/RAW"]}""";

        Assert.NotEqual(RoleJson.Hash(a), RoleJson.Hash(b));
    }

    [Fact]
    public void Hash_IsSensitiveToContent()
    {
        const string a = """{"role":"user","all":["FMO/RAW"]}""";
        const string b = """{"role":"user","all":["FMO/RAW","FMO/CUSTOM"]}""";

        Assert.NotEqual(RoleJson.Hash(a), RoleJson.Hash(b));
    }

    [Theory]
    [InlineData("")]
    [InlineData("{ this is not json")]
    [InlineData("null")]
    public void Hash_ReturnsNullForInvalidJson(string text)
    {
        Assert.Null(RoleJson.Hash(text));
    }
}
