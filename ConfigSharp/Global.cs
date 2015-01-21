namespace ConfigSharp
{
    public class Global
    {
        public static void Logger(LibLogger.CallbackToApplication sink) { Log.SetLogger(sink); }        
        public static Container Instance { get; set; }

        //public static int Get(string key, int defaultValue) { return Instance.Get(key, defaultValue); }
        //public static string Get(string key, string defaultValue) { return Instance.Get(key, defaultValue); }
        //public static long Get(string key, long defaultValue) { return Instance.Get(key, defaultValue); }
        //public static double Get(string key, double defaultValue) { return Instance.Get(key, defaultValue); }
        //public static bool Get(string key, bool defaultValue) { return Instance.Get(key, defaultValue); }
        //public static T Get<T>(string key) { return (T)Instance.Get<T>(key); }
    }
}
