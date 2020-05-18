/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Gauge.Dotnet
{
    internal class Logger
    {
        public static string SerializeLogInfo(LogInfo logInfo)
        {
            var serializer = new DataContractJsonSerializer(typeof(LogInfo));
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, logInfo);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
        private static void print(String level, String messsage, Boolean isError = false)
        {
            var l = new LogInfo();
            l.logLevel = level;
            l.message = messsage;
            var data = SerializeLogInfo(l);
            if (isError)
            {
                Console.Error.WriteLine(data);
            }
            else
            {
                Console.Out.WriteLine(data);
            }
        }

        internal static void Info(String message)
        {
            print("info", message);
        }
        internal static void Debug(String message)
        {
            print("debug", message);
        }
        internal static void Warning(String message)
        {
            print("warning", message);
        }
        internal static void Error(String message)
        {
            print("error", message, true);
        }
        internal static void Fatal(String message)
        {
            print("fatal", message, true);
            Environment.Exit(1);
        }
    }

    [DataContract]
    internal class LogInfo
    {
        [DataMember]
        public String logLevel { get; set; }

        [DataMember]
        public String message { get; set; }

    }
}