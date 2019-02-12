using ICM.Common.Helpers;
using ICM.FormatSupervisor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using NLog;
using Newtonsoft.Json;
using ICM.FormatSupervisor.Enums;
using System.IO;
using System.Reflection;

namespace ICM.FormatSupervisor.Services
{
    class SchemaRefCheck
    {
        [JsonProperty("$ref")]
        public string Ref { get; set; }
    }

    public class RuleService
    {
        protected readonly Logger Log = LogManager.GetCurrentClassLogger();
        private List<RuleModel> _rules = new List<RuleModel>();
            
        public async Task Load()
        {
            _rules = LoadRules(RuleType.Kafka, false);
            Log.Log(LogLevel.Info, $"{_rules.Count} Kafka rule(s) loaded");
        }

        public List<RuleModel> LoadRules(RuleType type, bool onlyEnabled)
        {
            var rules = new List<RuleModel>();

            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // disable schemas by making their extensions different from ".json"
            // file named "!all.json" will define schemas for every key under the topic
            var files = Directory.EnumerateFiles(Path.Combine(assemblyPath, $"Schemas\\{type}"), 
                onlyEnabled ? "*.json" : "*.*", SearchOption.AllDirectories).Select(i => new FileInfo(i));

            foreach (var fi in files)
            {
                var rule = new RuleModel
                {
                    Topic = fi.Directory.Name,
                    Key = Path.GetFileNameWithoutExtension(fi.Name) == "!all" ? null : Path.GetFileNameWithoutExtension(fi.Name).ToLower(),
                    RuleType = type,
                    Enabled = fi.Extension == ".json"
                };
                var text = File.ReadAllText(fi.FullName);
                var refCheck = JsonConvert.DeserializeObject<SchemaRefCheck>(text);
                if (refCheck.Ref != null)
                {
                    var schemaPath = Path.Combine(assemblyPath, $"Schemas\\{type}", refCheck.Ref);
                    if (!File.Exists(schemaPath))
                        throw new Exception($"Schema path failed. {fi.Directory.Name}/{fi.Name} => {refCheck.Ref}");
                    rule.Schema = JSchema.Parse(File.ReadAllText(schemaPath));
                }
                else
                    rule.Schema = JSchema.Parse(text);

                rules.Add(rule);
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
                return $"Not valid JSON";
            }

            var rule = GetRule(topic, key);
            if (rule == null)
                return null;

            if (SchemaExtensions.IsValid(msg, rule.Schema))
                return null;

            return $"Schema validation failed";
        }

        private RuleModel GetRule(string topic, string key)
        {
            return _rules.Where(i => i.Enabled && i.Topic == topic && (i.Key == null || i.Key == key))
                .OrderBy(i => string.IsNullOrEmpty(i.Key))
                .ThenBy(i => i.Key)
                .FirstOrDefault();
        }
    }
}
