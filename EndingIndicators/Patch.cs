using System;
using BepInEx;
using HarmonyLib;
using UnityEngine.UI;
using static EndingIndicators.Config;
using static SaveSlotCompletionIcons;

namespace EndingIndicators;

[BepInAutoPlugin(id: "unavailable.ending-indicators")]
public partial class Plugin : BaseUnityPlugin
{
    internal static Plugin _instance = null!;
    static Harmony _harmony = null!;

    void Awake()
    {
        Log.Debug("Mod loaded");

        _instance = this;
        _harmony = Harmony.CreateAndPatchAll(typeof(Patches));
    }
}

class Patches
{
    [HarmonyPatch(typeof(UIManager), nameof(UIManager.Awake))]
    static void Prefix()
    {
        // NOTE: If you setup the config on `Plugin.Awake()`, the SteamAPI would
        // still have not run, which would make `Setup()` to pick the wrong
        // config file path; read `Config.GetConfigFilePath()` for more details.
        Config.Setup(Plugin._instance);
    }

    [HarmonyPatch(
        typeof(SaveSlotCompletionIcons),
        nameof(SaveSlotCompletionIcons.SetCompletionIconState)
    )]
    static void Postfix(SaveSlotCompletionIcons __instance, SaveStats SaveStats)
    {
        var completionState = SaveStats.CompletedEndings;

        // NOTE: This is needed to support runtime reloading of the config.
        var atLeastOneCompletedEnding = false;
        foreach (var completionIcon in __instance.completionIcons)
        {
            var isEndingCompleted =
                completionIcon.state != CompletionState.None
                && completionState.HasFlag(completionIcon.state);

            atLeastOneCompletedEnding |= isEndingCompleted;

            if (Config.EndingDisplay is EndingDisplayOpts.OnlyCompleted)
            {
                completionIcon.icon.gameObject.SetActive(isEndingCompleted);
            }
            else if (
                Config.EndingDisplay
                is (EndingDisplayOpts.ShowAllIfAnyCompleted or EndingDisplayOpts.ShowAll)
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

            if (Config.NaturalOrder)
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
                    Config.EndingDisplay is EndingDisplayOpts.ShowAll || atLeastOneCompletedEnding
                );
        }
    }
}
