using BepInEx;
using HarmonyLib;
using static SaveSlotCompletionIcons;

namespace EndingIndicators;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private static Harmony _harmony = null!;

    private void Awake()
    {
        // TODO(Unavailable): Configuration:
        //  - Show not completed icons.
        //  - Include `MrMushroom` ending.
        //  - Reverse list order.
        _harmony = Harmony.CreateAndPatchAll(typeof(Patches));
    }
}

class Patches
{
    [HarmonyPatch(typeof(SaveSlotCompletionIcons), "SetCompletionIconState")]
    static void Postfix(SaveSlotCompletionIcons __instance, SaveStats SaveStats)
    {
        CompletionState completionState = SaveStats.CompletedEndings;
        foreach (var completionIcon in __instance.completionIcons)
        {
            completionIcon.icon.gameObject.SetActive(
                completionIcon.state != CompletionState.None
                    && completionState.HasFlag(completionIcon.state)
            );
        }
    }
}
