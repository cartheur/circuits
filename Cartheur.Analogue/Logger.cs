using System;
using System.Configuration;
using System.IO;

namespace SharpCircuit
{
    /// <summary>
    /// The class which performs logging for the application.
    /// </summary>
    public class Logging
    {
        public enum LogType { Information, Error };
        
        /// <summary>
        /// Logs the specified message using log file path from configuration settings.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="logType">Type of the log.</param>
        public static void WriteLog(string message, LogType logType)
        {
            var stream = new StreamWriter(ConfigurationManager.AppSettings["logFilePath"], true);
            switch (logType)
            {
                case LogType.Error:
                    stream.WriteLine(DateTime.Now + " - " + " ERROR " + " - " + message);
                    break;
                case LogType.Information:
                    stream.WriteLine(DateTime.Now + " - " + message);
                    break;
            }
            stream.Close();
            stream.Dispose();
        }
        /// <summary>
        /// Writes debug log with object parameters.
        /// </summary>
        /// <param name="objects">The objects.</param>
        public static void Debug(params object[] objects)
        {
            var stream = new StreamWriter(ConfigurationManager.AppSettings["logFilePath"], true);
            foreach (var obj in objects)
            {
                stream.WriteLine(obj);
            }
            stream.WriteLine("--");
            stream.Close();
            stream.Dispose();
        }
    }
}
