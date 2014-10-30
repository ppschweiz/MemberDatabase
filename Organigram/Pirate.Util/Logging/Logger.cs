/*
 * Copyright (c) 2009, Pirate Party Switzerland
 * All rights reserved.
 * 
 * Licensed under the New BSD License as seen in License.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pirate.Util.Logging
{
  /// <summary>
  /// Logs interesting events of the voting server.
  /// </summary>
  public class Logger : ILogger
  {
    private object _lock = new object();

    private string _logFileName;

    private string LogFileName
    {
      get { return string.Format("{0}_{1:d4}-{2:d2}-{3:d2}.log", _logFileName, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day); }
    }

    /// <summary>
    /// Maximum log level to log.
    /// </summary>
    private LogLevel _maxLogLevel;

    /// <summary>
    /// Create a new logger.
    /// </summary>
    /// <param name="logFileName">Name and path of the log file</param>
    /// <param name="maxLogLevel">Maximum log level to log.</param>
    public Logger(LogLevel maxLogLevel, string logFileName)
    {
      _maxLogLevel = maxLogLevel;
      _logFileName = logFileName;
    }

    /// <summary>
    /// Log a message to file.
    /// </summary>
    /// <param name="logLevel">Level of the message.</param>
    /// <param name="message">Message string.</param>
    /// <param name="values">Values to be inserted into the string.</param>
    public void Log(LogLevel logLevel, string message, params object[] values)
    {
      lock (_lock)
      {
        if (logLevel <= this._maxLogLevel)
        {
          string text = string.Format(message, values);

          if (text.Contains(Environment.NewLine))
          {
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            for (int index = 0; index < lines.Length; index++)
            {
              var line = lines[index];
              LogLine(logLevel, string.Format("{0}/{1} {2}", index + 1, lines.Length, line));
            }
          }
          else
          {
            LogLine(logLevel, text);
          }
        }
      }
    }

    private void LogLine(LogLevel logLevel, string line)
    {
      Console.WriteLine(line);

      if (Environment.OSVersion.Platform == PlatformID.Unix)
      {
        using (var syslog = new Syslog())
        {
          syslog.Log(LogLevelToUnix(logLevel), line);
          File.AppendAllText(LogFileName, Environment.NewLine + DateTime.Now.ToString() + " " + line);
        }
      }
    }

    private Syslog.SyslogSeverity LogLevelToUnix(LogLevel logLevel)
    {
      switch (logLevel)
      {
        case LogLevel.Emergency:
          return Syslog.SyslogSeverity.Alert;
        case LogLevel.Error:
          return Syslog.SyslogSeverity.Error;
        case LogLevel.Warning:
          return Syslog.SyslogSeverity.Warning;
        default:
          return Syslog.SyslogSeverity.Informational;
      }
    }
  }
}
