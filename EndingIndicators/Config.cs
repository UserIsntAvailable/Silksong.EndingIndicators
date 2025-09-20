using System.IO;
using BepInEx.Configuration;
using UnityEngine;

namespace EndingIndicators;

static class Config
{
    public enum EndingDisplayOpts
    {
        OnlyCompleted,
        ShowAllIfAnyCompleted,
        ShowAll,
    }

    static ConfigFile _configFile = null!;
    static ConfigFile ConfigFile =>
        _configFile is not null
            ? _configFile
            : _configFile = new ConfigFile(GetModConfigFilePath(), saveOnInit: true);

    // TODO(Unavailable): IncludeHeraldEnding

    static ConfigEntry<EndingDisplayOpts> _endingDisplay = null!;
    public static ConfigEntry<EndingDisplayOpts> EndingDisplay =>
        _endingDisplay is not null
            ? _endingDisplay
            : _endingDisplay = ConfigFile.Bind(
                "Options",
                nameof(EndingDisplay),
                EndingDisplayOpts.OnlyCompleted,
                "How endings should be displayed"
            );

    static ConfigEntry<bool> _naturalOrder = null!;
    public static ConfigEntry<bool> NaturalOrder =>
        _naturalOrder is not null
            ? _naturalOrder
            : _naturalOrder = ConfigFile.Bind(
                "Options",
                nameof(NaturalOrder),
                false,
                "Reverse the order of the endings to match their natural order of unlocking"
            );

    static string GetModConfigFilePath()
    {
        var platform = Platform.Current as DesktopPlatform;
        var userId = platform?.onlineSubsystem?.UserId;
        var accId = string.IsNullOrEmpty(userId) ? "default" : userId!;

        return Path.Combine(
            Application.persistentDataPath,
            accId,
            "plugins",
            PluginInfo.PLUGIN_GUID,
            $"config.dat"
        );
    }
}
