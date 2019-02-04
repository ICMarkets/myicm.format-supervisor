using Newtonsoft.Json.Schema;

namespace ICM.FormatSupervisor.Models
{
    public class RuleModel
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public string Key { get; set; }
        public JSchema Schema { get; set; }
        public bool Enabled { get; set; }
    }
}
