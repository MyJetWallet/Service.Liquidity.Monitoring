
namespace Service.Liquidity.Monitoring.Domain.Models.RuleSets
{
    public class CustomParam
    {
        public string Name { get; set; }
        public string Value { get; set; }

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