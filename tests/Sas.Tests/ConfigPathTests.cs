using Sas;

namespace Sas.Tests;

/// <summary>
/// Default configuration path resolution: --config → SAS_CONFIG → system config
/// (standard install directory only, when the file exists) → ~/.sas/config.json.
/// </summary>
public class ConfigPathTests
{
    [Fact]
    public void ExplicitPathWinsOverEverything()
    {
        var path = Config.ResolveConfigPath(
            explicitPath: "/tmp/custom.json",
            envPath: "/env/config.json",
            baseDirectory: "/opt/fmo-sas/",
            isWindows: false,
            fileExists: _ => true,
            homeDir: "/home/alice");

        Assert.Equal("/tmp/custom.json", path);
    }

    [Fact]
    public void EnvPathWinsOverSystemAndHome()
    {
        var path = Config.ResolveConfigPath(
            explicitPath: null,
            envPath: "/env/config.json",
            baseDirectory: "/opt/fmo-sas/",
            isWindows: false,
            fileExists: _ => true,
            homeDir: "/home/alice");

        Assert.Equal("/env/config.json", path);
    }

    [Fact]
    public void SystemConfigIsUsedWhenItExists()
    {
        var path = Config.ResolveConfigPath(
            explicitPath: null,
            envPath: null,
            baseDirectory: "/opt/fmo-sas/",
            isWindows: false,
            fileExists: p => p == "/etc/fmo-sas/config.json",
            homeDir: "/home/alice");

        Assert.Equal("/etc/fmo-sas/config.json", path);
    }

    [Fact]
    public void MissingSystemConfigFallsBackToHome()
    {
        var path = Config.ResolveConfigPath(
            explicitPath: null,
            envPath: null,
            baseDirectory: "/opt/fmo-sas",
            isWindows: false,
            fileExists: _ => false,
            homeDir: "/home/alice");

        Assert.Equal(Path.Combine("/home/alice", ".sas", "config.json"), path);
    }

    [Fact]
    public void NonStandardInstallDirectoryFallsBackToHome()
    {
        var path = Config.ResolveConfigPath(
            explicitPath: null,
            envPath: null,
            baseDirectory: "/opt/emqx/fmo-sas/",
            isWindows: false,
            fileExists: _ => true,
            homeDir: "/root");

        Assert.Equal(Path.Combine("/root", ".sas", "config.json"), path);
    }

    [Fact]
    public void MacOsStandardInstallDirectoryUsesLocalEtc()
    {
        var path = Config.ResolveConfigPath(
            explicitPath: null,
            envPath: null,
            baseDirectory: "/usr/local/opt/fmo-sas/",
            isWindows: false,
            fileExists: _ => true,
            homeDir: "/Users/alice");

        Assert.Equal("/usr/local/etc/fmo-sas/config.json", path);
    }

    [Fact]
    public void WindowsNeverUsesSystemConfig()
    {
        var path = Config.ResolveConfigPath(
            explicitPath: null,
            envPath: null,
            baseDirectory: "/opt/fmo-sas/",
            isWindows: true,
            fileExists: _ => true,
            homeDir: @"C:\Users\alice");

        Assert.Equal(Path.Combine(@"C:\Users\alice", ".sas", "config.json"), path);
    }

    [Theory]
    [InlineData("/opt/fmo-sas", "/etc/fmo-sas/config.json")]
    [InlineData("/opt/fmo-sas/", "/etc/fmo-sas/config.json")]
    [InlineData("/usr/local/opt/fmo-sas", "/usr/local/etc/fmo-sas/config.json")]
    [InlineData("/usr/local/opt/fmo-sas/", "/usr/local/etc/fmo-sas/config.json")]
    [InlineData("/opt/emqx/fmo-sas", null)]
    public void GetSystemConfigPath_OnlyMatchesStandardDirectories(string baseDirectory, string? expected)
    {
        Assert.Equal(expected, Config.GetSystemConfigPath(baseDirectory, isWindows: false));
    }

    [Fact]
    public void GetSystemConfigPath_WindowsReturnsNull()
    {
        Assert.Null(Config.GetSystemConfigPath("/opt/fmo-sas", isWindows: true));
    }
}
