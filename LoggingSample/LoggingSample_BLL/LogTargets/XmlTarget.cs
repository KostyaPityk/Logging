using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Xml.Linq;
using LoggingSample_Logs_DAL.Entities;

namespace LoggingSample_BLL.LogTargets
{
    [Target("XmlTarget")]
    public class XmlTarget : TargetWithLayout
    {
        private static object sync = new object();

        public XmlTarget()
        {
            Host = Environment.MachineName;
        }

        [RequiredParameter]
        public string Host { get; set; }

        protected override void Write(LogEventInfo logEvent)
        {
            string pathToLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            
            if (!Directory.Exists(pathToLog))
                Directory.CreateDirectory(pathToLog);
            
            string logFileName = Path.Combine(pathToLog, DateTime.Today.ToShortDateString() + ".xml");

            XDocument xdoc = null;
            XElement root = null;

            lock(sync)
            {
                if (!File.Exists(logFileName))
                {
                    xdoc = new XDocument();
                    root = new XElement("logs");
                    xdoc.Add(root);
                }
                else
                {
                    xdoc = XDocument.Load(logFileName);
                    root = xdoc.Root;
                }


                root.Add(CreateLogBlock(logEvent));
                xdoc.Save(logFileName);
            }
            
        }

        private XElement CreateLogBlock(LogEventInfo logEvent)
        {
            var logMessage = new LogMessage
            {
                MachineName = this.Host,
                Exception = logEvent.Exception?.ToString(),
                LoggerName = logEvent.LoggerName,
                Level = logEvent.Level.ToString(),
                Message = logEvent.Message,
                MessageSource = logEvent.CallerFilePath,
                TimeStamp = logEvent.TimeStamp
            };

            XElement newElement = new XElement("level",
                new XAttribute("type",logMessage.Level),
                new XElement("Exception", logMessage.Exception),
                new XElement("MachineName", logMessage.MachineName),
                new XElement("LoggerName", logMessage.LoggerName),
                new XElement("Message", logMessage.Message),
                new XElement("MessageSource", logMessage.MessageSource),
                new XElement("TimeStamp", logMessage.TimeStamp));

            return newElement; 
        }
    }
}
