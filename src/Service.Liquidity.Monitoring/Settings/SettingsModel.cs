using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Liquidity.Monitoring.Settings
{
    public class SettingsModel
    {
        [YamlProperty("Liquidity.Monitoring.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("Liquidity.Monitoring.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("Liquidity.Monitoring.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
    }
}
