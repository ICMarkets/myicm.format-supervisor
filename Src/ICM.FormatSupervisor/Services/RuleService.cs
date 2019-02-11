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
using Newtonsoft.Json;
using ICM.FormatSupervisor.Enums;

namespace ICM.FormatSupervisor.Services
{
    public class RuleService
    {
        protected readonly Logger Log = LogManager.GetCurrentClassLogger();
        private List<RuleModel> _rules = new List<RuleModel>();
            
        public async Task Load()
        {
            Log.Log(LogLevel.Debug, $"Loading rules (rules to clear: {_rules.Count})");
            _rules = await GetRulesFromDatabase(RuleType.Kafka, false);
            Log.Log(LogLevel.Debug, $"Rules loaded: {_rules.Count}");
        }

        public async Task<List<RuleModel>> GetRulesFromDatabase(RuleType type, bool onlyEnabled)
        {
            var rules = new List<RuleModel>();
            var connectionString = EnvironmentHelper.Variables[Variable.ICM_FORMATDB];

            using (var conn = new SqlConnection(connectionString))
            using (var command = new SqlCommand($@"
select r.Id, r.Topic, r.[Key], js.SchemaText, r.Enabled, r.RuleTypeId
from [Rule] r 
inner join [JsonSchema] js
on js.Id = r.JsonSchemaId
where r.RuleTypeId = {(int)type}" + (onlyEnabled ? " and r.Enabled = 1" : ""), conn))
            {
                await conn.OpenAsync();
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rules.Add(new RuleModel
                    {
                        Id = (int)reader["Id"],
                        Topic = GetWithNull<string>(reader["Topic"]),
                        Key = GetWithNull<string>(reader["Key"]),
                        Schema = JSchema.Parse((string)reader["SchemaText"]),
                        Enabled = (bool)reader["Enabled"],
                        RuleType = (RuleType)reader["RuleTypeId"],
                    });
                }
            }
            return rules;
        }

        private T GetWithNull<T>(object input)
        {
            if (input is DBNull)
                return default(T);
            if (input is T)
                return (T)input;
            throw new Exception("Invalid type");
        }

        public List<string> GetTopics()
        {
            return _rules.Select(i => i.Topic).Distinct().ToList();
        }

        public string Validate(string topic, string key, string message)
        {
            JObject msg;
            try
            {
                msg = JObject.Parse(message);
            }
            catch (Exception ex)
            {
                return $"Message {topic}/{key} is not a valid JSON";
            }

            var rule = GetRule(topic, key);
            if (rule == null)
                return null;

            if (SchemaExtensions.IsValid(msg, rule.Schema))
                return null;

            return $"Message {topic}/{key} rules failed: {rule.Id}";
        }

        private RuleModel GetRule(string topic, string key)
        {
            return _rules.Where(i => i.Enabled && i.Topic == topic && (i.Key == null || i.Key == key))
                .OrderBy(i => string.IsNullOrEmpty(i.Key))
                .ThenBy(i => i.Key)
                .SingleOrDefault();
        }
    }
}
