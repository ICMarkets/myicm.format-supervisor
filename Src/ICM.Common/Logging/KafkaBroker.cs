using NLog.Config;

namespace ICM.Common.Logging
{
    [NLogConfigurationItem]
    public class KafkaBroker
    {
        public KafkaBroker()
        {
            this.address = string.Empty;
        }
        public KafkaBroker(string address)
        {
            this.address = address;
        }
        [RequiredParameter]
        public string address { get; set; }
    }
}
