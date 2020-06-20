using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace ConfigSharp
{
    public static class Log
    {
        public enum Level
        {
            Silent,
            Error,
            Warning,
            Debug,
            User,
            Info,
            Verbose,
            Flooding,
        }

        public delegate void CallbackToApplication(Level level, string context, string message);

        internal static void Error(Exception ex, [CallerMemberName] string context = null, [CallerFilePath]string callerFilePath = null) { try { _Log(Level.Error, GetContext(context, callerFilePath), "Exception: " + ExceptionDetail(ex)); } catch (Exception) { } }

        internal static void Warning(Exception ex, [CallerMemberName] string context = null, [CallerFilePath]string callerFilePath = null) { try { _Log(Level.Warning, GetContext(context, callerFilePath), "Exception: " + ExceptionDetail(ex)); } catch (Exception) { } }
        internal static void Error(string message, [CallerMemberName] string context = null, [CallerFilePath]string callerFilePath = null) { try { _Log(Level.Error, GetContext(context, callerFilePath), message); } catch (Exception) { } }
        internal static void Warning(string message, [CallerMemberName] string context = null, [CallerFilePath]string callerFilePath = null) { try { _Log(Level.Warning, GetContext(context, callerFilePath), message); } catch (Exception) { } }
        internal static void Debug(string message, [CallerMemberName] string context = null, [CallerFilePath]string callerFilePath = null) { try { _Log(Level.Debug, GetContext(context, callerFilePath), message); } catch (Exception) { } }
        internal static void User(string message, [CallerMemberName] string context = null, [CallerFilePath]string callerFilePath = null) { try { _Log(Level.User, GetContext(context, callerFilePath), message); } catch (Exception) { } }
        internal static void Info(string message, [CallerMemberName] string context = null, [CallerFilePath]string callerFilePath = null) { try { _Log(Level.Info, GetContext(context, callerFilePath), message); } catch (Exception) { } }
        internal static void Verbose(string message, [CallerMemberName] string context = null, [CallerFilePath]string callerFilePath = null) { try { _Log(Level.Verbose, GetContext(context, callerFilePath), message); } catch (Exception) { } }
        internal static void Flooding(string message, [CallerMemberName] string context = null, [CallerFilePath]string callerFilePath = null) { try { _Log(Level.Flooding, GetContext(context, callerFilePath), message); } catch (Exception) { } }

        internal static bool IsVerbose => LogLevel >= Level.Verbose;
        internal static bool IsFlooding => LogLevel >= Level.Flooding;

        private struct LogLine
        {
            public readonly Level Level;
            public readonly string Context;
            public readonly string Message;

            public LogLine(Level level, string context, string message)
            {
                Level = level;
                Context = context;
                Message = message;
            }
        }
        private static readonly List<LogLine> Backlog = new List<LogLine>();

        internal static string LogFile = "";
        const string LogFileTempFolder = @"%TEMP%";
        internal static long LogFileLimit = 10 * 1024 * 1024;

        static CallbackToApplication _callback;
        public static CallbackToApplication LogHandler
        {
            get { return _callback; }
            set {
                _callback = value;

                ReplayBacklog();
            }
        }

        public static Level LogLevel = Level.Info;

        public static Level LevelFromString(string levels)
        {
            var lowerLevels = levels.ToLower();
            var maxLevel = Level.Silent;
            foreach (var level in typeof(Level).GetEnumValues()) {
                if (lowerLevels.Contains(level.ToString().ToLower())) {
                    if (maxLevel < (Level)level) {
                        maxLevel = (Level)level;
                    }
                }
            }
            return maxLevel;
        }

        private static readonly object Mutex = new object();

        private static int _pid = -1;
        private static int Pid { get { if (_pid == -1) { try { _pid = Process.GetCurrentProcess().Id; } catch (Exception) { } } return _pid; } set { _pid = value; } }

        internal static void _Log(Level level, string context, string message)
        {
            if (context == null) { context = ""; }
            if (message == null) { message = ""; }
            message = message.Replace("\r", "\\r").Replace("\n", "\\n");

            if (level <= LogLevel) {
                if (!string.IsNullOrEmpty(LogFile)) {
                    LogToFile(level, context, message);
                }

                if (_callback == null) {
                    AddToBacklog(level, context, message);
                }

                _callback?.Invoke(level, context, message);
            }
        }

        private static void AddToBacklog(Level level, string context, string message)
        {
            lock (Mutex) {
                Backlog?.Add(new LogLine(level, context, message));
            }
        }

        private static void ReplayBacklog()
        {
            lock (Mutex) {
                try {
                    if (Backlog != null) {
                        foreach (var line in Backlog.ToArray()) {
                            _callback?.Invoke(line.Level, line.Context, line.Message);
                        }
                    }
                } catch (Exception) {
                    // Dont let the logger crash the app
                }
            }
        }

        private static void LogToFile(Level level, string context, string message)
        {
            lock (Mutex) {
                try {
                    var logFile = LogFile;
                    logFile = logFile.Replace(LogFileTempFolder, Path.GetTempPath());

                    if (File.Exists(logFile) && LogFileLimit > 0 && new FileInfo(logFile).Length > LogFileLimit) {
                        RotateLogFile(logFile);
                    }

                    var now = DateTime.Now;
                    // ReSharper disable once LocalizableElement
                    File.AppendAllText(logFile, $"{now.ToString(CultureInfo.InvariantCulture)}.{now.Millisecond:D3} {Pid} " + level.ToString().Replace($"{nameof(Level.Debug)}", "#########") + $" {context} {message}" + Environment.NewLine);
                } catch (Exception) {
                    // file errors dont prevent other logging
                }
            } // lock
        }

        private static void RotateLogFile(string logFile)
        {
            var path = Path.GetDirectoryName(logFile);
            if (path != null) {
                var name = Path.GetFileName(logFile);
                var now = DateTime.Now;
                var newName = now.ToString("yyMMdd-HHmmss-") + now.Millisecond + "-" + name;
                var newPathName = Path.Combine(path, newName);
                File.Move(logFile, newPathName);
            }
        }

        private static string ExceptionDetail(Exception ex)
        {
            return string.Join(" | ", AllExceptionMessages(ex).ToArray()) + " | " + string.Join(" | ", InnerExceptionDetail(ex).ToArray());
        }

        private static string MethodName(int skip = 0)
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1 + skip);
            if (sf != null) {
                var mb = sf.GetMethod();
                if (mb != null && mb.DeclaringType != null) {
                    return mb.DeclaringType.FullName + "." + mb.Name;
                }
            }
            return "<unknown>";
        }

        private static string GetContext(string context, string callerFilePath)
        {
            if (context == null) {
                context = MethodName(3);
            }
            if (callerFilePath != null) {
                var guessedCallerTypeName = Path.GetFileNameWithoutExtension(callerFilePath);
                return guessedCallerTypeName + "." + context;
            }
            return null;
        }

        private static List<string> AllExceptionMessages(Exception self)
        {
            var result = new List<string>();

            var ex = self;
            var previousMessage = "";
            while (ex != null) {
                if (ex.Message != previousMessage) {
                    previousMessage = ex.Message;
                    result.Add(ex.Message);
                }
                ex = ex.InnerException;
            }

            return result;
        }

        private static List<string> InnerExceptionDetail(Exception self)
        {
            var result = new List<string>();

            var ex = self;
            if (self.InnerException != null) {
                ex = self.InnerException;
            }

            if (ex.Source != null) { result.Add("Source: " + ex.Source); }
            if (ex.StackTrace != null) { result.Add("Stack Trace: " + ex.StackTrace.Replace(Environment.NewLine, "\\n")); }
            if (ex.TargetSite != null) { result.Add("TargetSite: " + ex.TargetSite); }

            return result;
        }
    }
}
