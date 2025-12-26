using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Silksong.DataManager;
using Silksong.ModMenu;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Plugin;
using Silksong.ModMenu.Screens;
using UnityEngine.UI;
using static SaveSlotCompletionIcons;

namespace Silksong.EndingIndicators;

[BepInAutoPlugin(id: "io.github.userisntavailable.endingindicators")]
[BepInDependency(DataManagerPlugin.Id)]
[BepInDependency(ModMenuPlugin.Id)]
public partial class EndingIndicatorsPlugin
    : BaseUnityPlugin,
        IProfileDataMod<Config>,
        IModMenuCustomMenu
{
    static Harmony _harmony = null!;

    internal static EndingIndicatorsPlugin Instance { get; private set; } = null!;

    internal new Config Config { get; set; } = new();

    void Awake()
    {
        Log.Debug("Mod loaded");

        Instance = this;
        _harmony = new Harmony(Id);
        _harmony.PatchAll(typeof(Patches));
    }

    public Config? ProfileData
    {
        get => Config;
        set => Config = value ?? new();
    }

    public string ModMenuName() => Name.UnCamelCase();

    public AbstractMenuScreen BuildCustomMenu()
    {
        var config = this.Config;
        var menu = new SimpleMenuScreen(ModMenuName());

        // TODO(Unavailable): CheckBox
        ChoiceElement<bool> enabled = new(nameof(Config.Enabled), ChoiceModels.ForBool())
        {
            Value = config.Enabled,
        };
        enabled.OnValueChanged += (x) => config.Enabled = x;

        ChoiceElement<Config.EndingDisplayOpts> endingDisplay = new(
            nameof(Config.EndingDisplay).UnCamelCase(),
            ChoiceModels.ForEnum<Config.EndingDisplayOpts>(),
            "How the indicators should be displayed."
        )
        {
            Value = config.EndingDisplay,
        };
        endingDisplay.OnValueChanged += (x) => config.EndingDisplay = x;

        ChoiceElement<bool> naturalOrder = new(
            nameof(Config.NaturalOrder).UnCamelCase(),
            ChoiceModels.ForBool(),
            "Reverse the order of the endings to match their natural order of unlocking."
        );
        naturalOrder.OnValueChanged += (x) => config.NaturalOrder = x;

        menu.AddRange([enabled, endingDisplay, naturalOrder]);

        return menu;
    }
}

public record Config
{
    public enum EndingDisplayOpts
    {
        OnlyCompleted,
        ShowAllIfAnyCompleted,
        ShowAll,
    }

    public bool Enabled { get; set; } = true;

    [JsonConverter(typeof(StringEnumConverter))]
    public EndingDisplayOpts EndingDisplay { get; set; } = EndingDisplayOpts.OnlyCompleted;

    public bool NaturalOrder { get; set; } = false;

    // TODO(Unavailable): IncludeHeraldEnding
}

class Patches
{
    [HarmonyPatch(
        typeof(SaveSlotCompletionIcons),
        nameof(SaveSlotCompletionIcons.SetCompletionIconState)
    )]
    static void Postfix(SaveSlotCompletionIcons __instance, SaveStats SaveStats)
    {
        var config = EndingIndicatorsPlugin.Instance.Config;

        if (!config.Enabled)
            return;

        var completionState = SaveStats.CompletedEndings;

        // NOTE: This is needed to support runtime reloading of the config.
        var atLeastOneCompletedEnding = false;
        foreach (var completionIcon in __instance.completionIcons)
        {
            var isEndingCompleted =
                completionIcon.state != CompletionState.None
                && completionState.HasFlag(completionIcon.state);

            atLeastOneCompletedEnding |= isEndingCompleted;

            if (config.EndingDisplay is Config.EndingDisplayOpts.OnlyCompleted)
            {
                completionIcon.icon.gameObject.SetActive(isEndingCompleted);
            }
            else if (
                config.EndingDisplay
                is (
                    Config.EndingDisplayOpts.ShowAllIfAnyCompleted
                    or Config.EndingDisplayOpts.ShowAll
                )
            )
            {
                completionIcon.icon.gameObject.SetActive(true);
                if (!isEndingCompleted)
                {
                    var image = completionIcon.icon.GetComponent<Image>();
                    image.color = image.color with { a = .25f };
                }
            }

            if (config.NaturalOrder)
            {
                completionIcon.icon.transform.SetAsFirstSibling();
            }
            // NOTE: This is needed to support runtime reloading of the config.
            else
            {
                completionIcon.icon.transform.SetAsLastSibling();
            }
        }

        // NOTE: Just for good messure.
        if (__instance.completionIcons.Count > 0)
        {
            __instance
                .completionIcons[0]
                .icon.transform.parent.gameObject.SetActive(
                    config.EndingDisplay is Config.EndingDisplayOpts.ShowAll
                        || atLeastOneCompletedEnding
                );
        }
    }
}
