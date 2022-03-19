using System.Runtime.Serialization;

namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets
{
    [DataContract]
    public class CustomParam
    {
        [DataMember(Order = 1)] public string Name { get; set; }
        [DataMember(Order = 2)] public string Value { get; set; }

        public string GetString()
        {
            return Value;
        }

        public decimal GetDecimal()
        {
            return decimal.Parse(Value);
        }

        public int GetInt()
        {
            return int.Parse(Value);
        }
    }
}