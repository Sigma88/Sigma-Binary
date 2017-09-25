namespace SigmaBinaryPlugin
{
    internal static class Debug
    {
        internal static bool debug = false;
        static string Tag = "[SigmaLog SB]";

        internal static void Log(string message)
        {
            if (debug)
            {
                UnityEngine.Debug.Log(Tag + ": " + message);
            }
        }

        internal static void Log(string Method, string message)
        {
            if (debug)
            {
                UnityEngine.Debug.Log(Tag + " " + Method + ": " + message);
            }
        }
    }
}
