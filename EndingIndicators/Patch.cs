using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static EndingIndicators.Config;
using static SaveSlotCompletionIcons;

namespace EndingIndicators;

[BepInAutoPlugin(id: "unavailable.ending-indicators")]
public partial class Plugin : BaseUnityPlugin
{
    static Harmony _harmony = null!;

    void Awake()
    {
        _harmony = Harmony.CreateAndPatchAll(typeof(Patches));
    }
}

class Patches
{
    [HarmonyPatch(typeof(SaveSlotCompletionIcons), "SetCompletionIconState")]
    static void Postfix(SaveSlotCompletionIcons __instance, SaveStats SaveStats)
    {
        var completionState = SaveStats.CompletedEndings;

        if (
            Config.EndingDisplay.Value is EndingDisplayOpts.ShowAll
            // NOTE: Just for good messure.
            && __instance.completionIcons.Count > 0
        )
        {
            __instance.completionIcons[0].icon.transform.parent.gameObject.SetActive(true);
        }

        foreach (var completionIcon in __instance.completionIcons)
        {
            var isEndingCompleted =
                completionIcon.state != CompletionState.None
                && completionState.HasFlag(completionIcon.state);

            if (Config.EndingDisplay.Value is EndingDisplayOpts.OnlyCompleted)
            {
                completionIcon.icon.gameObject.SetActive(isEndingCompleted);
            }
            else if (
                Config.EndingDisplay.Value
                is (EndingDisplayOpts.ShowAllIfAnyCompleted or EndingDisplayOpts.ShowAll)
            )
            {
                completionIcon.icon.gameObject.SetActive(true);
                if (!isEndingCompleted)
                    completionIcon.icon.GetComponent<Image>().color -= new Color(0f, 0f, 0f, .75f);
            }
            else
            {
                throw new InvalidOperationException("Someone forgot to update this branch :)");
            }

            if (Config.NaturalOrder.Value)
            {
                completionIcon.icon.transform.SetAsFirstSibling();
            }
        }
    }
}
