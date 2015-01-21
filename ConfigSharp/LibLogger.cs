namespace ConfigSharp
{
    public static class LibLogger
    {
        public delegate void CallbackToApplication(string level, string message);
    }

    internal static class Log
    {
        internal static void Verbose(string message) { _Log("Verbose", message); }
        internal static void Info(string message) { _Log("Info", message); }
        internal static void Warning(string message) { _Log("Warning", message); }
        internal static void Error(string message) { _Log("Error", message); }

        private static LibLogger.CallbackToApplication _app;
        public static void SetLogger(LibLogger.CallbackToApplication app) { _app = app; }

        private static void _Log(string level, string message)
        {
            if (_app != null) {
                _app(level, message);
            }
        }

    }
}
