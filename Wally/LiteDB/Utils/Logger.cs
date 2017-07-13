﻿using System;

namespace LiteDB
{
    /// <summary>
    ///     A logger class to log all information about database. Used with levels. Level = 0 - 255
    ///     All log will be trigger before operation execute (better for debug)
    /// </summary>
    internal class Logger
    {
        public const byte NONE = 0;
        public const byte ERROR = 1;
        public const byte RECOVERY = 2;
        public const byte COMMAND = 4;
        public const byte QUERY = 16;
        public const byte JOURNAL = 32;
        public const byte DISK = 64;
        public const byte FULL = 255;

        public Logger()
        {
            Level = NONE;
        }

        /// <summary>
        ///     To full logger use Logger.FULL or any combination of Logger constants like Level = Logger.ERROR | Logger.COMMAND |
        ///     Logger.DISK
        /// </summary>
        public byte Level { get; set; }

        /// <summary>
        ///     Event when log writes a message. Fire on each log message
        /// </summary>
        public event Action<string> Logging;

        /// <summary>
        ///     Write log text to output using inside a component (statics const of Logger)
        /// </summary>
        public void Write(byte level, string message, params object[] args)
        {
            if ((level & Level) == 0) return;

            if (Logging != null)
            {
                string text = string.Format(message, args);

                string str =
                    level == ERROR
                        ? "ERROR"
                        : level == RECOVERY
                            ? "RECOVERY"
                            : level == COMMAND
                                ? "COMMAND"
                                : level == JOURNAL
                                    ? "JOURNAL"
                                    : level == DISK ? "DISK" : "QUERY";

                string msg = DateTime.Now.ToString("HH:mm:ss.ffff") + " [" + str + "] " + text;

                Logging(msg);
            }
        }
    }
}