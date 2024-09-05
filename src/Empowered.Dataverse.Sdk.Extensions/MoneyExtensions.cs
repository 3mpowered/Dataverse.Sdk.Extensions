using Microsoft.Xrm.Sdk;

namespace Empowered.Dataverse.Sdk.Extensions
{
    public static class MoneyExtensions
    {
        public static string Format(this Money money, string format = "G") =>
            money == null ? string.Empty : money.Value.ToString(format);
    }
}
