using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRM_TrashRecyclePopulation
    {
    public class WRMLogger
        {
        public string LogDirpath { get; set; }
        public string BaseLogFileName { get; set; }

        

        private static WRMLogger logger;
        public static WRMLogger Logger { get => logger; set => logger = value; }

        private static StringBuilder logBuilder = new StringBuilder();
        public static StringBuilder LogBuilder { get => logBuilder; set => logBuilder = value; }

        private static object lockLog = new Object();
      
        public WRMLogger(string dirPath, string logFileName)
            {
            LogDirpath = dirPath;
            BaseLogFileName = logFileName;
            }
        public void logMessageAndDeltaTime(string logLine, ref DateTime beforeNow, ref DateTime justNow, ref double loopMillisecondsPast)
            {
            
            justNow = DateTime.Now;
            TimeSpan timeDiff = justNow - beforeNow;
            loopMillisecondsPast += timeDiff.TotalMilliseconds;
            logBuilder.AppendLine(logLine + " From " + beforeNow.ToString("o", new CultureInfo("en-us")) + " To " + justNow.ToString("o", new CultureInfo("en-us")) + " MilliSeconds passed: " + timeDiff.TotalMilliseconds.ToString());
            beforeNow = justNow;
            }

        public void log()
            {
            lock (lockLog)
                {
                String currentDateString = DateTime.Now.ToString("yyyyMMdd");

                char separator = System.IO.Path.DirectorySeparatorChar;
                String logFilepath = String.Format("{0}{1}{2}-{3}.log", LogDirpath, System.IO.Path.DirectorySeparatorChar, BaseLogFileName, currentDateString);

                using (System.IO.FileStream fileStream = System.IO.File.Open(logFilepath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.None))
                    {

                    using (System.IO.StreamWriter logStream = new System.IO.StreamWriter(fileStream))
                        {
                        logStream.WriteLine(logBuilder);
                        }
                    }
                
                }
            logBuilder.Clear();
            }
        }
    }
