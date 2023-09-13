using Logging.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TwitchChatSpeaker.Logging
{
    /// <summary>
    /// Implementation of <see cref="ILogger"/> to write to a file
    /// </summary>
    internal class FileLogger : ILogger
    {
        private const string LogFileName = "TwitchChatSpeaker.log";

        private readonly BlockingCollection<string> logFileContents;
        private readonly Thread processingThread;

        /// <summary>
        /// Ctor for creating a <see cref="FileLogger"/>
        /// </summary>
        public FileLogger() 
        {
            try
            {
                if (File.Exists(LogFileName))
                {
                    File.Delete(LogFileName);
                }
            }
            catch (Exception ex) { }

            logFileContents = new BlockingCollection<string>(new ConcurrentQueue<string>());
            processingThread = new Thread(ProcessLogs)
            {
                IsBackground = true,
            };
            processingThread.Start();
        }

        /// <summary>
        /// Destructor to flush out remaining logs
        /// </summary>
        ~FileLogger()
        {
            try
            {
                Flush();
            }
            catch (Exception e) {}
        }

        /// <<inheritdoc/>
        public void Error(string message)
        {
            string finalMessage = $"ERR - {message}";
            logFileContents.Add(finalMessage);
        }

        /// <<inheritdoc/>
        public void Information(string message)
        {
            string finalMessage = $"INF - {message}";
            logFileContents.Add(finalMessage);
        }

        /// <<inheritdoc/>
        public void Warning(string message)
        {
            string finalMessage = $"WAR - {message}";
            logFileContents.Add(finalMessage);
        }

        /// <summary>
        /// Flush out any remaining logs to file
        /// </summary>
        public void Flush()
        {
            List<string> logsToPrint = new List<string>();
            while (logFileContents.TryTake(out string nextLog))
            {
                logsToPrint.Insert(0, nextLog);
            }

            // Write to file
            File.AppendAllLines(LogFileName, logsToPrint.ToArray());
        }

        /// <summary>
        /// Process logs at a set interval
        /// </summary>
        public void ProcessLogs()
        {
            while (true)
            {
                // Block on receiving a log
                string log = logFileContents.Take();
                List<string> logsToPrint = new List<string>() { log };

                // If we managed to get a log, check for another ten, exiting if we run out, or if we have reached over 10
                int counter = 0;
                while (logFileContents.TryTake(out string nextLog) && counter++ > 10)
                {
                    logsToPrint.Insert(0, nextLog);
                }
                
                // Write to file
                File.AppendAllLines(LogFileName, logsToPrint.ToArray());

                // Wait for a bit - 1s
                // This prevents us from hammering the disk if a lot of logs are received at once
                Thread.Sleep(1000);
            }
        }
    }
}
