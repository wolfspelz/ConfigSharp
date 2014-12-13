namespace ConfigSharp
{
    public class Global
    {
        public static void Logger(LibLogger.CallbackToApplication sink) { Log.SetLogger(sink); }        
        public static Container Instance { get; set; }

        //public static int Get(string sKey, int defaultValue) { return Instance.Get(sKey, defaultValue); }
        //public static string Get(string sKey, string defaultValue) { return Instance.Get(sKey, defaultValue); }
        //public static long Get(string sKey, long defaultValue) { return Instance.Get(sKey, defaultValue); }
        //public static double Get(string sKey, double defaultValue) { return Instance.Get(sKey, defaultValue); }
        //public static bool Get(string sKey, bool defaultValue) { return Instance.Get(sKey, defaultValue); }
        //public static T Get<T>(string sKey) { return (T)Instance.Get<T>(sKey); }
    }
}
