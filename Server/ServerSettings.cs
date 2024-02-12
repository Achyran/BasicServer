namespace ServerBase
{
    internal static class ServerSettings
    {
        //ToDo: Implement a logger and move the loglevel to the logger
        public enum LogLevel
        {
            silent = 0,
            basic = 1,
            All = 2
        }
        public static LogLevel logLevel = LogLevel.basic;
        public static int heartRate = 1000;
        public static int timeOutMs = 10000;
    }
}
