namespace ConfigSharp
{
    public static class LibLogger
    {
        public delegate void CallbackToApplication(string sLevel, string sMessage);
    }

    internal static class Log
    {
        internal static void Verbose(string sMessage) { _Log("Verbose", sMessage); }
        internal static void Info(string sMessage) { _Log("Info", sMessage); }
        internal static void Warning(string sMessage) { _Log("Warning", sMessage); }
        internal static void Error(string sMessage) { _Log("Error", sMessage); }

        private static LibLogger.CallbackToApplication _app;
        public static void SetLogger(LibLogger.CallbackToApplication app) { _app = app; }

        private static void _Log(string sLevel, string sMessage)
        {
            if (_app != null) {
                _app(sLevel, sMessage);
            }
        }

    }
}
