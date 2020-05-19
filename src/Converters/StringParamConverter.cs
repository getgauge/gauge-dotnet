/*----------------------------------------------------------------
 *  Copyright (c) ThoughtWorks, Inc.
 *  Licensed under the Apache License, Version 2.0
 *  See LICENSE.txt in the project root for license information.
 *----------------------------------------------------------------*/


using System.Linq;
using System.Reflection;
using Gauge.Messages;

namespace Gauge.Dotnet.Converters
{
    public class StringParamConverter : IParamConverter
    {
        public object Convert(Parameter parameter)
        {
            return parameter.Value;
        }

        public static object[] TryConvertParams(MethodInfo method, object[] parameters)
        {
            return method.GetParameters().Select((t, i) =>
            {
                try
                {
                    return System.Convert.ChangeType(parameters[i], t.ParameterType);
                }
                catch
                {
                    return parameters[i];
                }
            }).ToArray();
        }
    }
}