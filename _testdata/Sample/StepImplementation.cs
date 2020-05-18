/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Gauge.CSharp.Lib;
using Gauge.CSharp.Lib.Attribute;

namespace IntegrationTestSample
{
    public class StepImplementation
    {
        [Step("A context step which gets executed before every scenario")]
        public void Context()
        {
            Console.WriteLine("This is a sample context");
        }

        [Step("Say <what> to <who>")]
        public void SaySomething(string what, string who)
        {
            Console.WriteLine("{0}, {1}!", what, who);
            GaugeMessages.WriteMessage("{0}, {1}!", what, who);
        }

        [Step("I throw an unserializable exception")]
        public void ThrowUnserializableException()
        {
            throw new CustomException("I am a custom exception");
        }

        [Step("I throw a serializable exception")]
        public void ThrowSerializableException()
        {
            GaugeScreenshots.RegisterCustomScreenshotWriter(new StringScreenshotWriter());
            throw new CustomSerializableException("I am a custom serializable exception");
        }

        [Step("I throw a serializable exception and continue")]
        [ContinueOnFailure]
        public void ContinueOnFailure()
        {
            GaugeScreenshots.RegisterCustomScreenshotWriter(new StringScreenshotWriter());
            throw new CustomSerializableException("I am a custom serializable exception");
        }

        [Step("I throw an AggregateException")]
        public void AsyncExeption()
        {
            var tasks = new[]
            {
                Task.Run(() => { throw new CustomSerializableException("First Exception"); }),
                Task.Run(() => { throw new CustomSerializableException("Second Exception"); })
            };
            Task.WaitAll(tasks);
        }

        [Step("Step with text", "and an alias")]
        public void StepWithAliases()
        {
        }

        [Step("Step that takes a table <table>")]
        public void ReadTable(Table table)
        {
            var columnNames = table.GetColumnNames();
            columnNames.ForEach(Console.Write);
            var rows = table.GetTableRows();
            rows.ForEach(
                row => Console.WriteLine(columnNames.Select(row.GetCell)
                    .Aggregate((a, b) => string.Format("{0}|{1}", a, b))));
        }

        [Step("Take Screenshot in reference Project")]
        public void TakeProjectReferenceScreenshot() {
            GaugeScreenshots.RegisterCustomScreenshotWriter(new ReferenceProject.ScreenshotWriter());
            GaugeScreenshots.Capture();
            GaugeScreenshots.RegisterCustomScreenshotWriter(new StringScreenshotWriter());
        }

        [Step("Take Screenshot in reference DLL")]
        public void TakeDllReferenceScreenshot() {
            GaugeScreenshots.RegisterCustomScreenshotWriter(new ReferenceDll.ScreenshotWriter());
            GaugeScreenshots.Capture();
            GaugeScreenshots.RegisterCustomScreenshotWriter(new StringScreenshotWriter());
        }

        [Serializable]
        public class CustomSerializableException : Exception
        {
            public CustomSerializableException(string s) : base(s)
            {
            }

            public CustomSerializableException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }

        public class CustomException : Exception
        {
            public CustomException(string message) : base(message)
            {
            }
        }

        public class StringScreenshotWriter : ICustomScreenshotWriter
        {
            public string TakeScreenShot()
            {
                return "screenshot.png";
            }
        }
    }
}