using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeSeerAPI;

namespace HSPI_MagicHome
{
    public class Logger
    {
        private static ELogLevel m_logLevel = ELogLevel.Info;
        private static ELogLevel m_fileLogLevel = ELogLevel.Debug;
        private static bool m_logToFileEnabled;
        private static readonly object m_logToFileLock = new object();
        private static MagicHomeApp m_app;
        private static IHSApplication m_hs;
        private static StreamWriter m_fileWriter;

        public static ELogLevel LogLevel
        {
            get => m_logLevel;
            set
            {
                m_logLevel = value;
                m_hs?.SaveINISetting("GENERAL", "log_level", m_logLevel.ToString(), m_app.IniFile);
            }
        }

        public static ELogLevel FileLogLevel
        {
            get => m_fileLogLevel;
            set
            {
                m_fileLogLevel = value;
                m_hs?.SaveINISetting("GENERAL", "file_log_level", m_fileLogLevel.ToString(), m_app.IniFile);
            }
        }

        public static bool LogToFileEnabled
        {
            get => m_logToFileEnabled;
            set
            {
                if (value == m_logToFileEnabled)
                    return;
                m_logToFileEnabled = value;
                m_hs.SaveINISetting("GENERAL", "log_to_file_enabled", m_logToFileEnabled.ToString(), m_app.IniFile);
                if (m_logToFileEnabled)
                    LogFileInit();
                else
                    LogFileShut();
            }
        }

        public static void Init()
        {
            try
            {
                m_app = MagicHomeApp.GetInstance();
                m_hs = m_app.Hs;
                m_logToFileEnabled = Convert.ToBoolean(m_hs.GetINISetting("GENERAL", "log_to_file_enabled", "false", m_app.IniFile));
                m_logLevel = (ELogLevel)Enum.Parse(typeof(ELogLevel), m_hs.GetINISetting("GENERAL", "log_level", "Info", m_app.IniFile));
                m_fileLogLevel = (ELogLevel)Enum.Parse(typeof(ELogLevel), m_hs.GetINISetting("GENERAL", "file_log_level", "Debug", m_app.IniFile));
                LogFileInit();
                Trace.Listeners.Add(new MyTraceListener());
                AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }
        }

        public static void Shut()
        {
            try
            {
                LogFileShut();
                AppDomain.CurrentDomain.UnhandledException -= UnhandledExceptionTrapper;
                m_hs = null;
                m_app = null;
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
            }
        }

        private static void LogFileInit()
        {
            try
            {
                if (!m_logToFileEnabled)
                    return;
                if (!Directory.Exists("Logs"))
                    Directory.CreateDirectory("Logs");
                m_fileWriter?.Close();
                m_fileWriter = File.AppendText("Logs/MagicHome" + (string.IsNullOrEmpty(m_app.Instance) ? "" : "_" + MagicHomeApp.RemoveNonAlphanum(m_app.Instance)) + ".log");
            }
            catch (Exception ex)
            {
                m_fileWriter = null;
                LogError(ex.ToString());
            }
        }

        private static void LogFileShut()
        {
            try
            {
                if (m_fileWriter == null)
                    return;
                m_fileWriter.Close();
                m_fileWriter = null;
            }
            catch (Exception ex)
            {
                m_fileWriter = null;
                LogError(ex.ToString());
            }
        }

        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            LogDebug("Unhandled Exception Trapped");
            LogDebug(e.ExceptionObject.ToString());
            Environment.Exit(1);
        }

        private static void LogToFile(string line, ELogLevel level)
        {
            lock (m_logToFileLock)
            {
                try
                {
                    if (!m_logToFileEnabled || level < m_fileLogLevel || m_fileWriter == null)
                        return;
                    m_fileWriter.WriteLine(DateTime.Now.ToString("MMM-dd HH:mm:ss") + " " + level.ToString().ToUpper() + " " + line);
                    m_fileWriter.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public static void Log(string line, ELogLevel level)
        {
            try
            {
                LogToFile(line, level);
                if (level < m_logLevel)
                    return;
                Console.WriteLine(DateTime.Now.ToString("MMM-dd HH:mm:ss") + " " + level.ToString().ToUpper() + " " + line);
                if (m_hs == null)
                    return;
                m_hs.WriteLog("MagicHome", level.ToString().ToUpper() + " " + line);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void LogDebug(string line)
        {
            Log(line, ELogLevel.Debug);
        }

        // ReSharper disable once UnusedMember.Global
        public static void LogDebug(string format, params object[] args)
        {
            LogDebug(string.Format(format, args));
        }

        public static void LogInfo(string line)
        {
            Log(line, ELogLevel.Info);
        }

        public static void LogInfo(string format, params object[] args)
        {
            LogInfo(string.Format(format, args));
        }

        public static void LogWarning(string line)
        {
            Log(line, ELogLevel.Warning);
        }

        // ReSharper disable once UnusedMember.Global
        public static void LogWarning(string format, params object[] args)
        {
            LogWarning(string.Format(format, args));
        }

        public static void LogError(string line)
        {
            Log(line, ELogLevel.Error);
        }

        // ReSharper disable once UnusedMember.Global
        public static void LogError(string format, params object[] args)
        {
            LogError(string.Format(format, args));
        }

        public class MyTraceListener : TraceListener
        {
            public override void Write(string message)
            {
                LogDebug(message);
            }

            public override void WriteLine(string message)
            {
                LogDebug(message);
            }
        }

        public enum ELogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
        }
    }
}
