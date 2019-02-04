using NLog;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ICM.Common.Helpers
{
    /// <summary>
    /// Extend this enum to read required variables on startup
    /// </summary>
    /// <returns></returns>
    public enum Variable
    {
        ICM_SERVICENAME,  // Service name which identifies application, example: "Trinity"
        HOSTNAME,         // Used to identify specific inctance. Named HOSTNAME because of Docker unique id 
        ICM_CONFIG,       // Where to get service configuration
        ICM_ENVIRONMENT,  // Local / Development / Staging / Production etc

        ICM_FORMATDB,
        ICM_KAFKA
    }

    public static class EnvironmentHelper
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public static Dictionary<Variable, string> Variables { get; private set; }

        /// <summary>
        /// Defaults for env variables
        /// </summary>
        private static Dictionary<Variable, string> Defaults = new Dictionary<Variable, string>
        {
            { Variable.ICM_SERVICENAME, "formatsupervisor" },
            { Variable.HOSTNAME, Guid.NewGuid().ToString("d") },
            { Variable.ICM_CONFIG, "config.json" }, // assuming LocalConfigLoader
        };

        private static string GetEnvironmentVariable(string environmentVariableName, string defaultValue = null)
        {
            // for the winservice, it could be process variable, and in debug mode - user one
            var ret = Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.Process);
            if (ret == null)
                ret = Environment.GetEnvironmentVariable(environmentVariableName, EnvironmentVariableTarget.User);

            if (ret == null)
            {
                if (defaultValue == null)
                    Log.Log(LogLevel.Error, $"{environmentVariableName} environment variable is not set.");
                else
                    ret = defaultValue;
            }

            return ret;
        }

        public static void Load()
        {
            Variables = new Dictionary<Variable, string>();
            foreach (Variable variable in Enum.GetValues(typeof(Variable)))
            {
                string variableName = Enum.GetName(typeof(Variable), variable);
                var variableValue = GetEnvironmentVariable(variableName, GetDefault(variable));
                Variables.Add(variable, variableValue);
                LogManager.Configuration.Variables[variableName] = variableValue;
            }
        }

        private static string GetDefault(Variable variable)
        {
            if (Defaults.ContainsKey(variable))
                return Defaults[variable];
            return null;
        }
    }
}
