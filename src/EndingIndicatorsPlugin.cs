using System;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Silksong.DataManager;
using UnityEngine.UI;
using static SaveSlotCompletionIcons;

namespace Silksong.EndingIndicators;

[BepInAutoPlugin(id: "io.github.userisntavailable.endingindicators")]
[BepInDependency(DataManagerPlugin.Id)]
public partial class EndingIndicatorsPlugin : BaseUnityPlugin, IProfileDataMod<Config>
{
    static Harmony _harmony = null!;

    internal static EndingIndicatorsPlugin Instance { get; private set; } = null!;

    void Awake()
    {
        Log.Debug("Mod loaded");

        Instance = this;
        _harmony = new Harmony(Id);
        _harmony.PatchAll(typeof(Patches));
    }

    public Config? ProfileData { get; set; }
}

public record Config
{
    public enum EndingDisplayOpts
    {
        OnlyCompleted,
        ShowAllIfAnyCompleted,
        ShowAll,
    }

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
        var config = EndingIndicatorsPlugin.Instance.ProfileData ?? new();
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
            else
            {
                throw new InvalidOperationException("Someone forgot to add more branches :)");
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
