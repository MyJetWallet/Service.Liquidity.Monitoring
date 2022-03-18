using System.ComponentModel;

namespace Service.Liquidity.Monitoring.Domain.Models.Operators
{
    public enum CompareOperatorType
    {
        [Description("Bigger than")] Bigger,
        [Description("Less than")] Less,
        [Description("Equals to")] Equal,
    }
}