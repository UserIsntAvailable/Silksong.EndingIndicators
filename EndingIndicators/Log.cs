namespace EndingIndicators;

static class Log
{
    public static void Debug(string message)
    {
        UnityEngine.Debug.Log($"[{Plugin.Id}] {message}");
    }

    public static void Error(string message)
    {
        UnityEngine.Debug.LogError($"[{Plugin.Id}] {message}");
    }
}
