namespace Silksong.EndingIndicators;

static class Log
{
    // TODO(Unavailable): Take string templates
    public static void Debug(string message)
    {
        UnityEngine.Debug.Log($"[{EndingIndicatorsPlugin.Id}] {message}");
    }

    // TODO(Unavailable): Take string templates
    public static void Error(string message)
    {
        UnityEngine.Debug.LogError($"[{EndingIndicatorsPlugin.Id}] {message}");
    }
}
