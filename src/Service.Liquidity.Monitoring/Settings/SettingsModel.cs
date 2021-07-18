using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Liquidity.Monitoring.Settings
{
    public class SettingsModel
    {
        [YamlProperty("LiquidityMonitoring.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("LiquidityMonitoring.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("LiquidityMonitoring.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("LiquidityMonitoring.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }

        [YamlProperty("LiquidityMonitoring.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }
    }
}
