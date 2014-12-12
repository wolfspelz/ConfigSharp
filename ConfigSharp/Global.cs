namespace ConfigSharp
{
    public class Global
    {
        public static void Logger(LibLogger.CallbackToApplication sink) { Log.SetLogger(sink); }        
        public static Container Instance { get; set; }
    }
}
