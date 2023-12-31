﻿using System;
using FigNet.Core;
using Serilog;


namespace FigNetCommon
{

    public class DefaultLogger : FigNet.Core.ILogger
    {
        private Serilog.ILogger logger;

        public void SetUp(bool enableFileLogging, string fileName)
        {
            LoggerConfiguration config = new LoggerConfiguration();
            config.MinimumLevel.Debug();


            if (enableFileLogging)
            {
                config.WriteTo.File($"logs\\{fileName}.txt", rollOnFileSizeLimit: true, fileSizeLimitBytes: 10 * 1024 * 1024, rollingInterval: RollingInterval.Infinite, retainedFileCountLimit: 31);
            }
            else
            {
                config.WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information);
            }

            logger = config.CreateLogger();
            FN.LogMessageReceived += OnLogMessageReceived;
        }

        private void OnLogMessageReceived(string message, string exception, LogType logType)
        {
            switch (logType)
            {
                case LogType.Info:
                    Info(message);
                    break;
                case LogType.Warning:
                    Warning(message);
                    break;
                case LogType.Error:
                    Error(message);
                    break;
                case LogType.Exception:
                    Error(exception);
                    break;
            }
        }

        public void Error(string info)
        {
            logger.Error(info);
        }

        public void Error(string info, params object[] args)
        {
            logger.Error(info, args);
        }

        public void Exception(Exception exception, string info)
        {
            logger.Fatal(exception, info);
        }

        public void Exception(Exception exception, string info, params object[] args)
        {
            logger.Fatal(exception, info, args);
        }

        public void Info(string info)
        {
            logger.Information(info);
        }

        public void Info(string info, params object[] args)
        {
            logger.Information(info, args);
        }



        public void Warning(string info)
        {
            logger.Warning(info);
        }

        public void Warning(string info, params object[] args)
        {
            logger.Warning(info, args);
        }
    }
}
