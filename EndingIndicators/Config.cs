using System.ComponentModel;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;

namespace EndingIndicators;

static class Config
{
    public enum EndingDisplayOpts
    {
        [Description(nameof(EndingDisplayOpts.OnlyCompleted))]
        OnlyCompleted,

        [Description(nameof(EndingDisplayOpts.ShowAllIfAnyCompleted))]
        ShowAllIfAnyCompleted,

        [Description(nameof(EndingDisplayOpts.ShowAll))]
        ShowAll,
    }

    static ConfigFile _config = null!;

    // TODO(Unavailable): IncludeHeraldEnding

    static ConfigEntry<EndingDisplayOpts> _endingDisplay = null!;
    public static EndingDisplayOpts EndingDisplay => _endingDisplay.Value;

    static ConfigEntry<bool> _naturalOrder = null!;
    public static bool NaturalOrder => _naturalOrder.Value;

    // Reflection crimes incoming. Reader discretion needed.
    //
    // So, there is no currently a way to either change the file path of the
    // default config created by BepInEx, or tell `ConfigurationManager` to look
    // for custom file paths; read `GetConfigFilePath()` for the details of why
    // this is relevant.
    //
    // The former _might_ get fixed by https://github.com/BepInEx/BepInEx/pull/267,
    // but I doubt this is being merged any time soon. For the latter, the main
    // maintainer has expressed no interest in supporting custom file paths.
    // https://github.com/BepInEx/BepInEx.ConfigurationManager/issues/66#issuecomment-1546635871.
    //
    // So there is two ways to move forward. Either use reflection to patch
    // `BaseUnityPlugin.Config`, or to create a fork of `ConfigurationManager`
    // that supports custom file paths. I don't have any interest in maintaining
    // and recommending a custom fork, so for now I will wait and see what other
    // Silksong modders do.
    //
    // In the meantime, since `BepInEx` itself recommends the use of
    // `ConfigurationManager`, then reflection is really the only option (I
    // could of course just not support `ConfigurationManager`, but changing
    // settings in-game is way to convenient to not have it).
    public static void Setup(Plugin plugin)
    {
        if (_config is not null)
            return;

        var newConfig = new ConfigFile(
            GetConfigFilePath(),
            saveOnInit: false,
            plugin.Info.Metadata
        );

        var fieldInfo = plugin
            .GetType()
            .BaseType?.GetField(
                $"<{nameof(Plugin.Config)}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
        if (fieldInfo is not null)
        {
            fieldInfo.SetValue(plugin, newConfig);
        }
        else
        {
            Log.Error(
                "Patching of `Plugin.Config` failed. ConfigurationManager discovery will not work"
            );
        }

        _config = newConfig;
        _endingDisplay = _config.Bind(
            "Options",
            nameof(EndingDisplay),
            EndingDisplayOpts.OnlyCompleted,
            "When the ending indicators should be displayed"
        );
        _naturalOrder = _config.Bind(
            "Options",
            nameof(NaturalOrder),
            false,
            "Reverse the order of the endings to match their natural order of unlocking"
        );

        Log.Debug("Config parsed successfully");
    }

    // Steam Save Cloud allows modders to sync config files between devices
    // if the created files follow the developer's config file pattern (in this
    // case *.dat [it also searches files recursively]).
    static string GetConfigFilePath()
    {
        var platform = Platform.Current as DesktopPlatform;
        var userId = platform?.onlineSubsystem?.UserId;
        var accId = string.IsNullOrEmpty(userId) ? "default" : userId!;
        var configFilePath = Path.Combine(
            Application.persistentDataPath,
            accId,
            "plugins",
            Plugin.Id,
            $"config.dat"
        );

        Log.Debug($"Config file located at: {configFilePath}");

        return configFilePath;
    }
}
