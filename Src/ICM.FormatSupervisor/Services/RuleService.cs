using ICM.Common.Helpers;
using ICM.FormatSupervisor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using NLog;

namespace ICM.FormatSupervisor.Services
{
    public class RuleService
    {
        protected readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly List<RuleModel> _rules = new List<RuleModel>();
            
        public async Task Load()
        {
            Log.Log(LogLevel.Debug, $"Loading rules (rules to clear: {_rules.Count})");
            _rules.Clear();
            var connectionString = EnvironmentHelper.Variables[Variable.ICM_FORMATDB];

            // wtf magic
            using (var conn = new SqlConnection(connectionString)) { }
            //using (var command = new SqlCommand("select * from [Rule]", conn))
            //{
            //    var reader = await command.ExecuteReaderAsync();
            //    while (await reader.ReadAsync())
            //    {
            //        _rules.Add(new RuleModel
            //        {
            //            Id = (int)reader["Id"],
            //            Topic = (string)reader["Topic"],
            //            Key = (string)reader["Key"],
            //            Schema = JSchema.Parse((string)reader["Format"]),
            //            Enabled = (bool)reader["Enabled"]
            //        });
            //    }
            //}
            //Log.Log(LogLevel.Debug, $"Rules loaded: {_rules.Count})");
        }

        public List<string> GetTopics()
        {
            return _rules.Select(i => i.Topic).Distinct().ToList();
        }

        public string Validate(string topic, string key, string message)
        {
            var rules = GetRules(topic, key);

            var msg = JObject.Parse(message);

            bool isValid = false;

            foreach (var rule in rules)
            {
                IList<string> errors;
                if (SchemaExtensions.IsValid(msg, rule.Schema, out errors))
                {
                    isValid = true;
                    break;
                }
            }

            if (isValid)
                return null;

            return $"Message {topic}/{key} schemas failed: {Newtonsoft.Json.JsonConvert.SerializeObject(rules.Select(i => i.Id))}";
        }

        private List<RuleModel> GetRules(string topic, string key)
        {
            return _rules.Where(i => i.Enabled && i.Topic == topic && (i.Key == null || i.Key == key))
                .OrderBy(i => string.IsNullOrEmpty(i.Key))
                .ThenBy(i => i.Key)
                .ToList();
        }
    }
}
