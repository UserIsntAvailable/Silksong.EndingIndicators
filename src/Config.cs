using System.ComponentModel;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using GlobalEnums;
using UnityEngine;
using UnityEngine.UI;

namespace Silksong.EndingIndicators;

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

    // So, there is no currently a way to either change the file path of the
    // default config created by BepInEx, or tell `ConfigurationManager` to look
    // for custom file paths; read `GetConfigFilePath()` for the details of why
    // this is relevant.
    //
    // So, the simplest way to make `ConfigurationManager` discover the config
    // file, is to create a new `ConfigFile` with the appropiate path, and set
    // it throught reflection.
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
            """
            How the indicators should be displayed.
            Endings that are not yet completed, will be shown in a more transparent color
            if `ShowAllIfAnyCompleted` or `ShowAll` are used.
            """
        );
        _naturalOrder = _config.Bind(
            "Options",
            nameof(NaturalOrder),
            false,
            "Reverse the order of the endings to match their natural order of unlocking."
        );

        Log.Debug("Config parsed successfully");

        _config.SettingChanged += (_, args) =>
        {
            var uiManager = UIManager.instance;
            if (
                uiManager.uiState is UIState.MAIN_MENU_HOME
                && uiManager.menuState is MainMenuState.SAVE_PROFILES
            )
            {
                var saveSlots = GameObject.FindObjectsByType<SaveSlotButton>(
                    FindObjectsSortMode.None
                );
                foreach (var saveSlot in saveSlots)
                {
                    saveSlot.saveSlotCompletionIcons.SetCompletionIconState(saveSlot.saveStats);
                }

                var key = args.ChangedSetting.Definition.Key;
                Log.Debug($"{key} setting changed. Live UI reload applied");
            }
        };
    }

    // Putting things into `Application.persistentDataPath` (in this case, this
    // is the `SAVEFILES` directory), allows Steam Save Cloud to pick up the files
    // in order to share files between devices; just make sure the files finish
    // on `*.dat`.
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
