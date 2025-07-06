using VRage.Utils;

namespace HardStart
{
    public static class Log
    {
        const string Prefix = "HardStart";

        public static bool DebugLog;
        public static void Msg(string msg)
        {
            MyLog.Default.WriteLine($"{Prefix}: {msg}");
        }

        public static void Debug(string msg)
        {
            if (DebugLog)
                Msg($"[DEBUG] {msg}");
        }
    }
}
